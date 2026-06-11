<script setup lang="ts">
import { ref, onMounted, watch, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { MdEditor, NormalToolbar } from 'md-editor-v3'
import 'md-editor-v3/lib/style.css'
import MarkdownEditor from '@/components/MarkdownEditor.vue'
import request from '@/utils/request'
import type { CategoryListItem } from '@/types/category'
import type { TagListItem } from '@/types/tag'
import type { CreateArticleRequest, UpdateArticleRequest } from '@/types/article'
import { getArticleApi, createArticleApi, updateArticleApi, aiExtractArticleApi, aiGenerateCoverApi, aiPolishArticleApi, aiWriteArticleApi, aiGenerateArticleImageApi } from '@/api/article'
import { getCategoryListApi } from '@/api/category'
import { getTagListApi } from '@/api/tag'
import { toSlug } from '@/utils/slug'
import MediaSelector from '@/components/MediaSelector.vue'
import CoverCropper from '@/components/CoverCropper.vue'

const route = useRoute()
const router = useRouter()

const isEdit = ref(false)
const articleId = ref<string | null>(null)
const loading = ref(false)
const saving = ref(false)
const aiExtracting = ref(false)
const aiGenCoverLoading = ref(false)

// AI 润色
const aiPolishDialogVisible = ref(false)
const aiPolishLoading = ref(false)
const aiPolishGenerateImages = ref(false)
const aiPolishAspectRatio = ref('16:9')

// AI 一键写文
const aiWriteDialogVisible = ref(false)
const aiWriteLoading = ref(false)
const aiWriteInspiration = ref('')
const aiWriteGenCover = ref(true)
const aiWriteGenArticleImages = ref(false)
const aiWriteArticleImageAspectRatio = ref('16:9')

// AI 插入配图（编辑器光标位置）
const aiInsertImageDialogVisible = ref(false)
const aiInsertImageLoading = ref(false)
const aiInsertImagePrompt = ref('')
const aiInsertImageAspectRatio = ref('16:9')

const aspectRatioOptions = [
  { label: '16:9 横屏', value: '16:9' },
  { label: '9:16 竖屏', value: '9:16' },
  { label: '4:3 横屏', value: '4:3' },
  { label: '3:4 竖屏', value: '3:4' },
  { label: '1:1 方形', value: '1:1' },
]

/** 表单数据 */
const form = ref({
  title: '',
  slug: '',
  summary: '',
  content: '',
  coverImageId: null as string | null,
  categoryIds: [] as string[],
  tagIds: [] as string[],
  isTop: false,
  isRecommend: false,
  status: 'draft' as string,
  seoTitle: '',
  seoDescription: '',
  seoKeywords: ''
})

/** 封面图预览 URL */
const coverPreviewUrl = ref('')

/** 封面裁剪弹窗 */
const coverCropperVisible = ref(false)
const cropperFile = ref<File | null>(null)
const cropperImageUrl = ref('')
const cropperFromMedia = ref(false)

/** 是否正在为封面选择媒体库图片 */
const coverSelecting = ref(false)

/** 表单校验 */
const formRef = ref()
const rules = {
  title: [{ required: true, message: '请输入文章标题', trigger: 'blur' }],
  slug: [
    { required: true, message: '请输入 URL 标识', trigger: 'blur' },
    { pattern: /^[a-z0-9-]+$/, message: '仅允许小写字母、数字和连字符', trigger: 'blur' }
  ],
  content: [{ required: true, message: '请输入文章内容', trigger: 'change' }]
}

/** 分类选项（树形） */
const categories = ref<CategoryListItem[]>([])

/** 标签选项 */
const tags = ref<TagListItem[]>([])

/** 编辑器引用 */
const editorRef = ref()

/** Slug 自动生成（中文转拼音） */
function autoGenerateSlug() {
  if (isEdit.value) return
  const title = form.value.title.trim()
  if (title) {
    form.value.slug = toSlug(title)
  }
}

/** 加载选项数据 */
async function loadOptions() {
  try {
    const [catRes, tagRes] = await Promise.all([
      getCategoryListApi(),
      getTagListApi()
    ])
    // 分类列表：使用树形结构，el-tree-select 会递归渲染子节点
    categories.value = catRes.data.data || []
    tags.value = tagRes.data.data || []
  } catch {
    // ignore
  }
}

/** 上传封面图（先打开裁剪弹窗） */
async function handleCoverUpload(file: File) {
  const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp']
  if (!allowedTypes.includes(file.type)) {
    ElMessage.warning('仅支持 jpg/png/gif/webp 格式')
    return false
  }
  if (file.size > 10 * 1024 * 1024) {
    ElMessage.warning('图片大小不能超过 10MB')
    return false
  }

  // 生成临时预览 URL 并打开裁剪弹窗
  cropperFile.value = file
  cropperImageUrl.value = URL.createObjectURL(file)
  coverCropperVisible.value = true
  return false
}

/** 封面裁剪完成回调 */
function onCoverCropped(url: string, id: string) {
  if (id === '__skip_crop__') {
    // 媒体库选择且已是 16:9，直接使用
    form.value.coverImageId = '__media__'
    coverPreviewUrl.value = url
    ElMessage.success('封面图已选择')
  } else {
    form.value.coverImageId = id
    coverPreviewUrl.value = url
    ElMessage.success('封面图上传成功（已裁剪为 16:9）')
  }
  // 释放临时 URL（本地上传模式）
  if (cropperImageUrl.value && cropperImageUrl.value.startsWith('blob:')) {
    URL.revokeObjectURL(cropperImageUrl.value)
  }
  cropperImageUrl.value = ''
  cropperFile.value = null
  cropperFromMedia.value = false
}

/** 移除封面图 */
function handleRemoveCover() {
  form.value.coverImageId = null
  coverPreviewUrl.value = ''
}

/** 加载文章（编辑模式） */
async function loadArticle(id: string) {
  loading.value = true
  try {
    const res = await getArticleApi(id)
    const data = res.data.data
    if (data) {
      form.value = {
        title: data.title,
        slug: data.slug,
        summary: data.summary || '',
        content: data.content,
        coverImageId: data.coverImageId || null,
        categoryIds: data.categories?.map((c: any) => c.id) || [],
        tagIds: data.tags.map((t: any) => t.id),
        isTop: data.isTop,
        isRecommend: data.isRecommend,
        status: data.status,
        seoTitle: data.seoTitle || '',
        seoDescription: data.seoDescription || '',
        seoKeywords: data.seoKeywords || ''
      }
      coverPreviewUrl.value = data.coverImageUrl || ''
    }
  } catch {
    ElMessage.error('加载文章失败')
    router.push('/articles')
  } finally {
    loading.value = false
  }
}

/** 保存文章 */
async function handleSave(status: string) {
  form.value.status = status

  // 内容不能为空
  if (!form.value.content.trim()) {
    ElMessage.warning('请输入文章内容')
    return
  }

  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return

  saving.value = true
  try {
    if (isEdit.value) {
      await updateArticleApi(articleId.value!, form.value as UpdateArticleRequest)
      ElMessage.success(status === 'published' ? '文章已发布' : '草稿已保存')
    } else {
      await createArticleApi(form.value as CreateArticleRequest)
      ElMessage.success(status === 'published' ? '文章已发布' : '草稿已创建')
    }
    router.push('/articles')
  } catch {
    // 错误已在拦截器中处理
  } finally {
    saving.value = false
  }
}

/** AI 提取文章信息（标题、摘要、SEO） */
async function handleAiExtract() {
  if (!form.value.content.trim()) {
    ElMessage.warning('请先输入文章内容')
    return
  }

  aiExtracting.value = true
  try {
    const res = await aiExtractArticleApi({
      content: form.value.content,
      currentTitle: form.value.title || undefined
    })
    if (res.data?.code === 200 && res.data.data) {
      const d = res.data.data
      if (d.title && !form.value.title) form.value.title = d.title
      if (d.summary) form.value.summary = d.summary
      if (d.seoTitle) form.value.seoTitle = d.seoTitle
      if (d.seoDescription) form.value.seoDescription = d.seoDescription
      if (d.seoKeywords) form.value.seoKeywords = d.seoKeywords

      // 自动生成 slug
      if (form.value.title) {
        form.value.slug = toSlug(form.value.title)
      }

      ElMessage.success('AI 信息提取完成')
    }
  } catch (err: any) {
    console.error('AI 信息提取失败:', err)
    if (err?.response?.data?.message) {
      ElMessage.error(err.response.data.message)
    } else if (err?.message && !err.message.includes('cancel')) {
      ElMessage.error('AI 信息提取失败: ' + (err.message || '未知错误'))
    }
  } finally {
    aiExtracting.value = false
  }
}
/** AI 生成文章封面图 */
async function handleAiGenCover() {
  if (!form.value.content.trim()) {
    ElMessage.warning('请先输入文章内容')
    return
  }

  aiGenCoverLoading.value = true
  try {
    const res = await aiGenerateCoverApi({
      title: form.value.title || '未命名文章',
      content: form.value.content
    })
    if (res.data?.code === 200 && res.data.data) {
      form.value.coverImageId = res.data.data.mediaId
      coverPreviewUrl.value = res.data.data.url
      ElMessage.success('AI 封面生成完成')
    }
  } catch (err: any) {
    console.error('AI 生成封面失败:', err)
    if (err?.response?.data?.message) {
      ElMessage.error(err.response.data.message)
    } else if (err?.message && !err.message.includes('cancel')) {
      ElMessage.error('AI 生成封面失败: ' + (err.message || '未知错误'))
    }
  } finally {
    aiGenCoverLoading.value = false
  }
}

/** AI 润色文章内容 — 弹出选项框让用户选择 */
function handleAiPolish() {
  if (!form.value.content.trim()) {
    ElMessage.warning('请先输入文章内容')
    return
  }
  aiPolishGenerateImages.value = false
  aiPolishAspectRatio.value = '16:9'
  aiPolishDialogVisible.value = true
}

/** 执行 AI 润色（用户点击确认后） */
async function confirmAiPolish() {
  aiPolishDialogVisible.value = false
  aiPolishLoading.value = true
  try {
    const res = await aiPolishArticleApi({
      content: form.value.content,
      generateImages: aiPolishGenerateImages.value,
      aspectRatio: aiPolishGenerateImages.value ? aiPolishAspectRatio.value : undefined
    })
    if (res.data?.code === 200 && res.data.data) {
      form.value.content = res.data.data.content
      ElMessage.success(aiPolishGenerateImages.value ? 'AI 润色并配图完成' : 'AI 润色完成')
    } else {
      const msg = (res.data as any)?.message || '润色失败'
      ElMessage.error(msg)
    }
  } catch (err: any) {
    console.error('AI 润色失败:', err)
    if (err?.response?.data?.message) {
      ElMessage.error(err.response.data.message)
    } else if (err?.message && !err.message.includes('cancel')) {
      ElMessage.error('AI 润色失败: ' + (err.message || '未知错误'))
    }
  } finally {
    aiPolishLoading.value = false
  }
}

/** 插入 AI 配图到编辑器光标位置 */
function TencentInsertImage() {
  console.log('[AI配图] 打开弹窗')
  aiInsertImagePrompt.value = ''
  aiInsertImageAspectRatio.value = '16:9'
  aiInsertImageDialogVisible.value = true
  console.log('[AI配图] aiInsertImageDialogVisible =', aiInsertImageDialogVisible.value)
}

async function handleAiInsertImage() {
  if (!aiInsertImagePrompt.value.trim()) {
    ElMessage.warning('请输入图片提示词')
    return
  }

  aiInsertImageLoading.value = true
  try {
    const res = await aiGenerateArticleImageApi({
      prompt: aiInsertImagePrompt.value.trim(),
      aspectRatio: aiInsertImageAspectRatio.value
    })
    if (res.data?.code === 200 && res.data.data) {
      const d = res.data.data
      const mdImg = `\n\n![${aiInsertImagePrompt.value}](${d.url})\n\n`
      // 使用 insert 回调方式插入到编辑器光标位置
      editorRef.value?.insert((_selectedText: string) => ({
        targetValue: mdImg,
        select: false,
      }))
      aiInsertImageDialogVisible.value = false
      ElMessage.success('AI 配图已插入')
    } else {
      const msg = (res.data as any)?.message || '生成失败'
      ElMessage.error(msg)
    }
  } catch (err: any) {
    console.error('AI 插入配图失败:', err)
    if (err?.response?.data?.message) {
      ElMessage.error(err.response.data.message)
    } else if (err?.message && !err.message.includes('cancel')) {
      ElMessage.error('AI 插入配图失败: ' + (err.message || '未知错误'))
    }
  } finally {
    aiInsertImageLoading.value = false
  }
}

/** AI 一键写文 */
async function handleAiWrite() {
  if (!aiWriteInspiration.value.trim()) {
    ElMessage.warning('请输入灵感或想法')
    return
  }

  aiWriteLoading.value = true
  try {
    const res = await aiWriteArticleApi({
      inspiration: aiWriteInspiration.value,
      generateCover: aiWriteGenCover.value,
      generateArticleImages: aiWriteGenArticleImages.value,
      articleImageAspectRatio: aiWriteArticleImageAspectRatio.value
    })
    if (res.data?.code === 200 && res.data.data) {
      const d = res.data.data
      form.value.title = d.title
      form.value.summary = d.summary
      form.value.content = d.content
      form.value.seoTitle = d.seoTitle
      form.value.seoDescription = d.seoDescription
      form.value.seoKeywords = d.seoKeywords
      if (d.title) {
        form.value.slug = toSlug(d.title)
      }

      if (d.coverUrl) {
        form.value.coverImageId = d.coverMediaId || null
        coverPreviewUrl.value = d.coverUrl
      }

      aiWriteDialogVisible.value = false
      ElMessage.success('AI 一键写文完成')
    } else {
      const msg = (res.data as any)?.message || '写文失败'
      ElMessage.error(msg)
    }
  } catch (err: any) {
    console.error('AI 一键写文失败:', err)
    if (err?.response?.data?.message) {
      ElMessage.error(err.response.data.message)
    } else if (err?.message && !err.message.includes('cancel')) {
      ElMessage.error('AI 一键写文失败: ' + (err.message || '未知错误'))
    }
  } finally {
    aiWriteLoading.value = false
  }
}

/** 打开 AI 一键写文弹窗 */
function TencentWrite() {
  console.log('[AI写文] 打开弹窗')
  aiWriteInspiration.value = ''
  aiWriteGenCover.value = true
  aiWriteGenArticleImages.value = false
  aiWriteArticleImageAspectRatio.value = '16:9'
  aiWriteDialogVisible.value = true
  console.log('[AI写文] aiWriteDialogVisible =', aiWriteDialogVisible.value)
}

onMounted(async () => {
  await loadOptions()

  if (route.name === 'ArticleEdit') {
    isEdit.value = true
    articleId.value = route.params.id as string
    await loadArticle(articleId.value)
  }
})

/** ===== 增强编辑器：从媒体库插入图片/视频 ===== */

/** 媒体插入对话框 */
const mediaDialogVisible = ref(false)
const mediaType = ref<'image' | 'video' | 'file'>('image')

/** 打开媒体选择对话框 */
function openMediaSelector(type: 'image' | 'video' | 'file') {
  mediaType.value = type
  coverSelecting.value = false
  mediaDialogVisible.value = true
}

/** 打开媒体选择对话框（封面图模式） */
function openCoverMediaSelector() {
  mediaType.value = 'image'
  coverSelecting.value = true
  mediaDialogVisible.value = true
}

/** 检测图片是否为 16:9（±2% 容差） */
function checkImageRatio(width: number, height: number): boolean {
  const ratio = width / height
  const target = 16 / 9
  return Math.abs(ratio - target) / target < 0.02
}

/** 从媒体库插入内容（关闭对话框 → 通过 CodeMirror EditorView 插入到光标位置） */
function insertMediaUrl(url: string, mediaId?: string) {
  // 如果是在为封面图选择媒体库图片
  if (coverSelecting.value) {
    coverSelecting.value = false
    mediaDialogVisible.value = false

    // 加载图片检查比例
    const img = new Image()
    img.onload = () => {
      if (checkImageRatio(img.naturalWidth, img.naturalHeight)) {
        // 已经是 16:9，直接使用
        form.value.coverImageId = mediaId || null
        coverPreviewUrl.value = url
        ElMessage.success('封面图已选择')
      } else {
        // 需要裁剪，打开裁剪弹窗
        cropperFromMedia.value = true
        cropperFile.value = null
        cropperImageUrl.value = url
        coverCropperVisible.value = true
      }
    }
    img.onerror = () => {
      // 无法加载时直接使用
      form.value.coverImageId = mediaId || null
      coverPreviewUrl.value = url
      ElMessage.success('封面图已选择')
    }
    img.src = url
    return
  }

  let markdown = ''
  if (mediaType.value === 'video') {
    markdown = `\n<video src="${url}" controls style="max-width:100%"></video>\n`
  } else if (mediaType.value === 'file') {
    markdown = `\n<a href="${url}" target="_blank" rel="noopener noreferrer">下载附件</a>\n`
  } else {
    markdown = `\n![图片](${url})\n`
  }

  // 先关闭对话框
  mediaDialogVisible.value = false

  // 等 DOM 更新后，通过 EditorView 插入到光标位置
  nextTick(() => {
    const view = editorRef.value?.getEditorView()
    if (view) {
      const pos = view.state.selection.main.head
      view.dispatch({
        changes: { from: pos, insert: markdown }
      })
      view.focus()
    } else {
      // 降级：追加到末尾
      form.value.content += markdown
    }
    ElMessage.success('已插入到编辑器')
  })
}

/** 上传视频文件 */
async function handleVideoUpload(file: File) {
  const maxSize = 200 * 1024 * 1024 // 200MB
  if (file.size > maxSize) {
    ElMessage.warning('视频文件不能超过 200MB')
    return false
  }

  try {
    const formData = new FormData()
    formData.append('file', file)
    const res = await request.post('/admin/upload/video', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    if (res.data.success && res.data.data?.url) {
      const videoUrl = res.data.data.url
      form.value.content += `\n<video src="${videoUrl}" controls style="max-width:100%"></video>\n`
      ElMessage.success('视频已插入，请在编辑器中调整位置')
    }
  } catch {
    ElMessage.error('视频上传失败')
  }
  return false
}

/** 上传附件文件 */
async function handleFileUpload(file: File) {
  const maxSize = 100 * 1024 * 1024 // 100MB
  if (file.size > maxSize) {
    ElMessage.warning('附件不能超过 100MB')
    return false
  }

  try {
    const formData = new FormData()
    formData.append('file', file)
    const res = await request.post('/admin/upload/file', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    if (res.data.success && res.data.data?.url) {
      form.value.content += `\n<a href="${res.data.data.url}" target="_blank" rel="noopener noreferrer">下载附件</a>\n`
      ElMessage.success('附件已插入，请在编辑器中调整位置')
    }
  } catch {
    ElMessage.error('附件上传失败')
  }
  return false
}

/** 编辑器内图片上传处理（通过工具栏图片按钮触发） */
async function handleEditorUploadImg(files: File[], callBack: (urls: string[]) => void) {
  try {
    const urls = await Promise.all(
      files.map(async (file) => {
        const formData = new FormData()
        formData.append('file', file)
        const resp = await request.post('/admin/upload/image', formData, {
          headers: { 'Content-Type': 'multipart/form-data' }
        })
        if (resp.data.success && resp.data.data?.url) {
          return resp.data.data.url
        }
        throw new Error('上传失败')
      })
    )
    callBack(urls)
  } catch {
    ElMessage.error('图片上传失败')
  }
}

/** 编辑器工具栏视频上传按钮点击处理 */
function handleVideoToolbarClick() {
  const input = document.createElement('input')
  input.type = 'file'
  input.accept = 'video/mp4,video/webm,video/mov,video/avi,video/mkv'
  input.onchange = async (e: Event) => {
    const target = e.target as HTMLInputElement
    const file = target.files?.[0]
    if (!file) return

    const maxSize = 200 * 1024 * 1024
    if (file.size > maxSize) {
      ElMessage.warning('视频文件不能超过 200MB')
      return
    }

    try {
      const formData = new FormData()
      formData.append('file', file)
      const res = await request.post('/admin/upload/video', formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })
      if (res.data.success && res.data.data?.url) {
        editorRef.value?.insertText(`<video src="${res.data.data.url}" controls style="max-width:100%"></video>\n`)
        ElMessage.success('视频已插入到编辑器中')
      }
    } catch {
      ElMessage.error('视频上传失败')
    }
  }
  input.click()
}
</script>

<template>
  <div class="editor-page">
    <!-- 页面标题 -->
    <div class="page-header">
      <h3 class="page-title">{{ isEdit ? '编辑文章' : '新建文章' }}</h3>
      <div class="header-actions">
        <el-button @click="router.push('/articles')">取消</el-button>
        <el-button
          type="primary"
          :loading="saving"
          @click="handleSave('draft')"
        >
          保存草稿
        </el-button>
        <el-button
          type="success"
          :loading="saving"
          @click="handleSave('published')"
        >
          发布
        </el-button>
        <el-divider direction="vertical" />
        <el-button type="warning" plain @click="TencentWrite">
          <el-icon><svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2l2.4 7.2h7.6l-6 4.8 2.4 7.2-6-4.8-6 4.8 2.4-7.2-6-4.8h7.6z"/></svg></el-icon> AI 一键写文
        </el-button>
      </div>
    </div>

    <div class="editor-body" v-loading="loading">
      <div class="editor-main">
        <!-- 基本信息 -->
        <el-card shadow="hover" class="section-card">
          <template #header>基本信息</template>

          <el-form ref="formRef" :model="form" :rules="rules" label-width="100px">
            <el-form-item label="文章标题" prop="title">
              <el-input
                v-model="form.title"
                placeholder="请输入文章标题"
                maxlength="255"
                @input="autoGenerateSlug"
              />
            </el-form-item>

            <el-form-item label="URL 标识" prop="slug">
              <el-input v-model="form.slug" placeholder="小写字母、数字和连字符" maxlength="255" />
            </el-form-item>

            <el-form-item label="文章摘要" prop="summary">
              <el-input
                v-model="form.summary"
                type="textarea"
                :rows="3"
                placeholder="文章摘要，将显示在文章卡片中"
                maxlength="512"
                show-word-limit
              />
            </el-form-item>

            <el-form-item label="分类">
              <el-tree-select
                v-model="form.categoryIds"
                :data="categories"
                multiple
                check-strictly
                show-checkbox
                placeholder="选择分类（可多选）"
                style="width: 100%"
                node-key="id"
                :props="{
                  label: 'name',
                  children: 'children',
                  value: 'id'
                }"
                filterable
              />
            </el-form-item>

            <el-form-item label="标签">
              <el-select
                v-model="form.tagIds"
                multiple
                placeholder="选择标签"
                style="width: 100%"
              >
                <el-option v-for="t in tags" :key="t.id" :label="t.name" :value="t.id">
                  <el-tag :color="t.color || '#409eff'" size="small" style="color: #fff">
                    {{ t.name }}
                  </el-tag>
                </el-option>
              </el-select>
            </el-form-item>

            <el-form-item label="属性">
              <el-checkbox v-model="form.isTop" label="置顶" border />
              <el-checkbox v-model="form.isRecommend" label="推荐" border style="margin-left: 8px" />
            </el-form-item>
          </el-form>

          <div style="text-align: right; margin-top: 8px;">
            <el-button size="small" type="primary" :loading="aiExtracting" @click="handleAiExtract">
              <el-icon><svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2l2.4 7.2h7.6l-6 4.8 2.4 7.2-6-4.8-6 4.8 2.4-7.2-6-4.8h7.6z"/></svg></el-icon> AI 信息提取
            </el-button>
          </div>
        </el-card>

        <!-- Markdown 编辑器 -->
        <el-card shadow="hover" class="section-card">
          <template #header>
            <div class="editor-toolbar-header">
              <span>正文内容</span>
              <div class="editor-extra-tools">
                <el-upload
                  :show-file-list="false"
                  :auto-upload="false"
                  accept="video/mp4,video/webm,video/ogg"
                  :on-change="(f: any) => handleVideoUpload(f.raw!)"
                  class="toolbar-upload"
                >
                  <el-button size="small">
                    <el-icon><VideoCamera /></el-icon>上传视频
                  </el-button>
                </el-upload>
                <!-- 上传附件 -->
                <el-upload
                  :show-file-list="false"
                  :auto-upload="false"
                  accept=".pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.zip,.rar,.7z,.txt,.csv,.json,.xml,.md"
                  :on-change="(f: any) => handleFileUpload(f.raw!)"
                  class="toolbar-upload"
                >
                  <el-button size="small">
                    <el-icon><Upload /></el-icon>上传附件
                  </el-button>
                </el-upload>
                <el-button size="small" @click="openMediaSelector('image')">
                  <el-icon><Picture /></el-icon>媒体库-图片
                </el-button>
                <el-button size="small" @click="openMediaSelector('video')">
                  <el-icon><VideoCamera /></el-icon>媒体库-视频
                </el-button>
                <el-button size="small" @click="openMediaSelector('file')">
                  <el-icon><FolderOpened /></el-icon>媒体库-附件
                </el-button>
                <el-divider direction="vertical" />
                <el-button size="small" plain @click="TencentInsertImage">
                  <el-icon><svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="18" height="18" rx="2"/><circle cx="8.5" cy="8.5" r="1.5"/><path d="M21 15l-5-5L5 21"/></svg></el-icon> 插入 AI 图
                </el-button>
                <el-button size="small" type="warning" :loading="aiPolishLoading" @click="handleAiPolish">
                  <el-icon><svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2l2.4 7.2h7.6l-6 4.8 2.4 7.2-6-4.8-6 4.8 2.4-7.2-6-4.8h7.6z"/></svg></el-icon> AI 润色
                </el-button>
              </div>
            </div>
          </template>

          <el-form-item prop="content" class="editor-form-item">
            <MarkdownEditor
              ref="editorRef"
              v-model="form.content"
              :min-height="500"
              placeholder="请输入 Markdown 内容..."
            />
          </el-form-item>
        </el-card>
      </div>

      <div class="editor-sidebar">
        <!-- SEO 设置 -->
        <el-card shadow="hover" class="section-card">
          <template #header>SEO 设置</template>
          <el-form label-width="80px">
            <el-form-item label="SEO 标题">
              <el-input v-model="form.seoTitle" placeholder="留空则使用文章标题" maxlength="255" />
            </el-form-item>
            <el-form-item label="SEO 描述">
              <el-input
                v-model="form.seoDescription"
                type="textarea"
                :rows="3"
                placeholder="可选"
                maxlength="512"
                show-word-limit
              />
            </el-form-item>
            <el-form-item label="SEO 关键词">
              <el-input
                v-model="form.seoKeywords"
                placeholder="多个关键词用逗号分隔"
                maxlength="255"
              />
            </el-form-item>
          </el-form>
        </el-card>

        <!-- 封面图设置 -->
        <el-card shadow="hover" class="section-card">
          <template #header>封面图</template>
          <div class="cover-upload">
            <template v-if="coverPreviewUrl">
              <div class="cover-preview">
                <img :src="coverPreviewUrl" alt="封面图预览" />
                <div class="cover-actions">
                  <el-upload
                    :show-file-list="false"
                    :auto-upload="false"
                    :on-change="(uploadFile: any) => handleCoverUpload(uploadFile.raw!)"
                    accept="image/jpeg,image/png,image/gif,image/webp"
                  >
                    <el-button size="small" type="primary">替换</el-button>
                  </el-upload>
                  <el-button size="small" @click="openCoverMediaSelector">从媒体库选择</el-button>
                  <el-button size="small" type="danger" @click="handleRemoveCover">移除</el-button>
                  <el-button size="small" type="warning" :loading="aiGenCoverLoading" @click="handleAiGenCover">
                    <el-icon><svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2l2.4 7.2h7.6l-6 4.8 2.4 7.2-6-4.8-6 4.8 2.4-7.2-6-4.8h7.6z"/></svg></el-icon> AI 生成
                  </el-button>
                </div>
              </div>
            </template>
            <template v-else>
              <el-upload
                drag
                :show-file-list="false"
                :auto-upload="false"
                :on-change="(uploadFile: any) => handleCoverUpload(uploadFile.raw!)"
                accept="image/jpeg,image/png,image/gif,image/webp"
              >
                <el-icon class="el-icon--upload" :size="40">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
                    <polyline points="17 8 12 3 7 8" />
                    <line x1="12" y1="3" x2="12" y2="15" />
                  </svg>
                </el-icon>
                <div class="el-upload__text">
                  将封面图拖到此处，或<em>点击上传</em>
                </div>
                <template #tip>
                  <div class="el-upload__tip">
                    建议尺寸 1920×1080（16:9），支持 jpg/png/webp，上传后自动裁剪为 16:9，最大 10MB
                  </div>
                </template>
              </el-upload>
              <div style="margin-top:8px;text-align:center">
                <el-button size="small" @click="openCoverMediaSelector">从媒体库选择</el-button>
                <el-button size="small" type="warning" :loading="aiGenCoverLoading" @click="handleAiGenCover">AI 生成封面</el-button>
              </div>
            </template>
          </div>
        </el-card>

        <!-- 发布设置 -->
        <el-card shadow="hover" class="section-card">
          <template #header>发布设置</template>
          <div class="publish-info">
            <div class="info-item">
              <span class="info-label">当前状态</span>
              <el-tag v-if="form.status === 'published'" type="success">已发布</el-tag>
              <el-tag v-else-if="form.status === 'archived'" type="danger">已下架</el-tag>
              <el-tag v-else type="info">草稿</el-tag>
            </div>

            <div class="info-item" v-if="isEdit">
              <span class="info-label">文章 ID</span>
              <span class="info-value">{{ articleId }}</span>
            </div>
          </div>

          <div class="publish-actions">
            <el-button
              type="primary"
              :loading="saving"
              style="width: 100%"
              @click="handleSave('draft')"
            >
              保存草稿
            </el-button>
            <el-button
              type="success"
              :loading="saving"
              style="width: 100%; margin-left: 0; margin-top: 8px"
              @click="handleSave('published')"
            >
              发布文章
            </el-button>
          </div>
        </el-card>
      </div>
    </div>

    <!-- 媒体选择对话框 -->
    <MediaSelector
      v-model:visible="mediaDialogVisible"
      :type="mediaType"
      @insert="insertMediaUrl"
    />

    <!-- 封面裁剪弹窗 -->
    <CoverCropper
      v-model:visible="coverCropperVisible"
      :file="cropperFile"
      :image-url="cropperImageUrl"
      :from-media="cropperFromMedia"
      @success="onCoverCropped"
    />

    <!-- AI 一键写文弹窗 -->
    <el-dialog v-model="aiWriteDialogVisible" title="AI 一键写文" width="560px" :close-on-click-modal="false">
      <el-form label-width="100px">
        <el-form-item label="灵感/想法">
          <el-input
            v-model="aiWriteInspiration"
            type="textarea"
            :rows="6"
            placeholder="输入你的灵感、想法或粗糙内容，AI 将帮你扩充为一篇完整的文章..."
          />
        </el-form-item>
        <el-form-item label="生成封面图">
          <el-switch v-model="aiWriteGenCover" />
          <span style="margin-left:8px;color:#909399;font-size:12px">自动生成文章封面图</span>
        </el-form-item>
        <el-form-item label="文章内配图">
          <el-switch v-model="aiWriteGenArticleImages" />
          <span style="margin-left:8px;color:#909399;font-size:12px">AI 自动在段落间插入配图</span>
        </el-form-item>
        <el-form-item v-if="aiWriteGenArticleImages" label="配图比例">
          <el-select v-model="aiWriteArticleImageAspectRatio" style="width: 100%">
            <el-option v-for="opt in aspectRatioOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="aiWriteDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="aiWriteLoading" @click="handleAiWrite">
          <el-icon><svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2l2.4 7.2h7.6l-6 4.8 2.4 7.2-6-4.8-6 4.8 2.4-7.2-6-4.8h7.6z"/></svg></el-icon> 开始生成
        </el-button>
      </template>
    </el-dialog>

    <!-- AI 润色选项弹窗 -->
    <el-dialog v-model="aiPolishDialogVisible" title="AI 润色" width="420px" :close-on-click-modal="false">
      <el-form label-width="120px">
        <el-form-item label="自动插入配图">
          <el-switch v-model="aiPolishGenerateImages" />
          <span style="margin-left:8px;color:#909399;font-size:12px">在段落之间自动生成并插入配图</span>
        </el-form-item>
        <el-form-item v-if="aiPolishGenerateImages" label="配图比例">
          <el-select v-model="aiPolishAspectRatio" style="width: 100%">
            <el-option label="16:9 横屏" value="16:9" />
            <el-option label="9:16 竖屏" value="9:16" />
            <el-option label="4:3 横屏" value="4:3" />
            <el-option label="3:4 竖屏" value="3:4" />
            <el-option label="1:1 方形" value="1:1" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="aiPolishDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="aiPolishLoading" @click="confirmAiPolish">
          <el-icon><svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2l2.4 7.2h7.6l-6 4.8 2.4 7.2-6-4.8-6 4.8 2.4-7.2-6-4.8h7.6z"/></svg></el-icon> 确认润色
        </el-button>
      </template>
    </el-dialog>

    <!-- AI 插入配图弹窗 -->
    <el-dialog v-model="aiInsertImageDialogVisible" title="AI 插入配图" width="500px" :close-on-click-modal="false">
      <el-form label-width="80px">
        <el-form-item label="提示词" required>
          <el-input
            v-model="aiInsertImagePrompt"
            type="textarea"
            :rows="4"
            placeholder="用英文描述你想要的图片内容，如：a modern office workspace with natural lighting"
          />
        </el-form-item>
        <el-form-item label="图片比例">
          <el-select v-model="aiInsertImageAspectRatio" style="width: 100%">
            <el-option v-for="opt in aspectRatioOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="aiInsertImageDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="aiInsertImageLoading" @click="handleAiInsertImage">
          <el-icon><svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="18" height="18" rx="2"/><circle cx="8.5" cy="8.5" r="1.5"/><path d="M21 15l-5-5L5 21"/></svg></el-icon> 生成并插入
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.editor-page {
  padding: 0;
}
.editor-toolbar-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 8px;
}
.editor-extra-tools {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
  align-items: center;
}
.toolbar-upload {
  display: inline-flex;
  align-items: center;
}
.media-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 12px;
}
.media-item {
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  overflow: hidden;
  cursor: pointer;
  transition: border-color 0.2s;
}
.media-item:hover {
  border-color: #409eff;
}
.media-video-placeholder {
  height: 120px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background: #f5f7fa;
  gap: 8px;
}
.media-name {
  font-size: 12px;
  color: #666;
  max-width: 100%;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  padding: 0 8px;
}
.media-check-label {
  text-align: center;
  font-size: 12px;
  padding: 4px;
  background: #f0f5ff;
  color: #409eff;
}
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}
.page-title {
  margin: 0;
  font-size: 20px;
}
.header-actions {
  display: flex;
  gap: 8px;
}
.editor-body {
  display: flex;
  gap: 16px;
  align-items: flex-start;
}
.editor-main {
  flex: 1;
  min-width: 0;
}
.editor-sidebar {
  width: 340px;
  flex-shrink: 0;
}
.section-card {
  margin-bottom: 16px;
}
.editor-form-item {
  margin-bottom: 0;
}
.publish-info {
  margin-bottom: 16px;
}
.info-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 0;
  border-bottom: 1px solid #f0f0f0;
}
.info-item:last-child {
  border-bottom: none;
}
.info-label {
  font-size: 14px;
  color: #909399;
}
.info-value {
  font-size: 14px;
  color: #303133;
  word-break: break-all;
}
.publish-actions {
  margin-top: 8px;
}
.cover-upload {
  min-height: 100px;
}
.cover-preview {
  position: relative;
  border-radius: 4px;
  overflow: hidden;
}
.cover-preview img {
  width: 100%;
  height: auto;
  display: block;
  border-radius: 4px;
}
.cover-actions {
  display: flex;
  gap: 8px;
  margin-top: 8px;
}
.cover-actions .el-upload {
  display: inline-flex;
}
.el-icon--upload {
  margin-bottom: 8px;
  color: #c0c4cc;
}
</style>