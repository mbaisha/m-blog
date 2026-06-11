<script setup lang="ts">
import { ref, onMounted, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { MdEditor, NormalToolbar } from 'md-editor-v3'
import 'md-editor-v3/lib/style.css'
import MarkdownEditor from '@/components/MarkdownEditor.vue'
import request from '@/utils/request'
import { getProjectApi, createProjectApi, updateProjectApi } from '@/api/project'
import { aiGenerateCoverApi, aiGenerateArticleImageApi, aiPolishArticleApi } from '@/api/article'
import { toSlug } from '@/utils/slug'
import MediaSelector from '@/components/MediaSelector.vue'
import CoverCropper from '@/components/CoverCropper.vue'

const route = useRoute()
const router = useRouter()

const isEdit = ref(false)
const projectId = ref<string | null>(null)
const loading = ref(false)
const saving = ref(false)

const form = ref({
  title: '',
  slug: '',
  summary: '',
  content: '',
  coverImageUrl: '',
  coverImageId: null as string | null,
  techStack: '',
  projectUrl: '',
  githubUrl: '',
  demoUrl: '',
  sortOrder: 0,
  isVisible: true
})

const formRef = ref()
const rules = {
  title: [{ required: true, message: '请输入项目标题', trigger: 'blur' }],
  slug: [
    { required: true, message: '请输入 URL 标识', trigger: 'blur' },
    { pattern: /^[a-z0-9-]+$/, message: '仅允许小写字母、数字和连字符', trigger: 'blur' }
  ]
}

const editorRef = ref()

// AI 封面图生成
const aiGenCoverLoading = ref(false)

// AI 润色
const aiPolishDialogVisible = ref(false)
const aiPolishLoading = ref(false)
const aiPolishGenerateImages = ref(false)
const aiPolishAspectRatio = ref('16:9')

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

/** 封面裁剪弹窗 */
const coverCropperVisible = ref(false)
const cropperFile = ref<File | null>(null)
const cropperImageUrl = ref('')
const cropperFromMedia = ref(false)

/** Slug 自动生成（中文转拼音） */
function autoGenerateSlug() {
  if (isEdit.value) return
  const title = form.value.title.trim()
  if (title) form.value.slug = toSlug(title)
}

/** 上传封面图（先打开裁剪弹窗） */
async function handleCoverUpload(file: any) {
  const allowedTypes = ['image/jpeg', 'image/png', 'image/webp']
  if (!allowedTypes.includes(file.raw?.type || file.type)) {
    ElMessage.warning('仅支持 jpg/png/webp 格式')
    return false
  }
  const maxSize = 10 * 1024 * 1024
  if (file.size > maxSize) {
    ElMessage.warning('封面图不能超过 10MB')
    return false
  }

  const rawFile = file.raw || file
  // 生成临时预览 URL 并打开裁剪弹窗
  cropperFile.value = rawFile
  cropperImageUrl.value = URL.createObjectURL(rawFile)
  coverCropperVisible.value = true
  return false
}

/** 封面裁剪完成回调 */
function onCoverCropped(url: string, id: string) {
  if (id === '__skip_crop__') {
    // 媒体库选择且已是 16:9，直接使用
    form.value.coverImageUrl = url
    form.value.coverImageId = '__media__'
    ElMessage.success('封面图已选择')
  } else {
    form.value.coverImageUrl = url
    form.value.coverImageId = id
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

async function loadProject(id: string) {
  loading.value = true
  try {
    const res = await getProjectApi(id)
    const data = res.data.data
    if (data) {
      form.value = {
        title: data.title,
        slug: data.slug,
        summary: data.summary || '',
        content: data.content || '',
        coverImageUrl: data.coverImageUrl || '',
        coverImageId: data.coverImageId || null,
        techStack: (data.techStack || []).join(', '),
        projectUrl: data.projectUrl || '',
        githubUrl: data.githubUrl || '',
        demoUrl: data.demoUrl || '',
        sortOrder: data.sortOrder,
        isVisible: data.isVisible
      }
    }
  } catch {
    ElMessage.error('加载项目详情失败')
    router.push('/projects')
  } finally {
    loading.value = false
  }
}

async function handleSave() {
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return

  saving.value = true
  try {
    const techStack = form.value.techStack
      ? form.value.techStack.split(',').map((s: string) => s.trim()).filter(Boolean)
      : []

    const data = {
      title: form.value.title,
      slug: form.value.slug,
      summary: form.value.summary || null,
      content: form.value.content || null,
      coverImageId: form.value.coverImageId || null,
      techStack,
      projectUrl: form.value.projectUrl || null,
      githubUrl: form.value.githubUrl || null,
      demoUrl: form.value.demoUrl || null,
      sortOrder: form.value.sortOrder,
      isVisible: form.value.isVisible
    }

    if (isEdit.value && projectId.value) {
      await updateProjectApi(projectId.value, data)
      ElMessage.success('项目更新成功')
    } else {
      await createProjectApi(data)
      ElMessage.success('项目创建成功')
    }
    router.push('/projects')
  } catch {
    // ignore
  } finally {
    saving.value = false
  }
}

function handleCancel() {
  router.push('/projects')
}

/** ===== 编辑器增强：上传图片/视频 + 媒体库 ===== */

const mediaDialogVisible = ref(false)
const mediaType = ref<'image' | 'video' | 'file'>('image')
const coverSelecting = ref(false)

function openMediaSelector(type: 'image' | 'video' | 'file') {
  mediaType.value = type
  coverSelecting.value = false
  mediaDialogVisible.value = true
}

/** 从媒体库选择封面图 */
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

/** 从媒体库插入内容 */
function insertMediaUrl(url: string, mediaId?: string) {
  if (coverSelecting.value) {
    coverSelecting.value = false
    mediaDialogVisible.value = false

    // 加载图片检查比例
    const img = new Image()
    img.onload = () => {
      if (checkImageRatio(img.naturalWidth, img.naturalHeight)) {
        // 已经是 16:9，直接使用
        form.value.coverImageUrl = url
        form.value.coverImageId = mediaId || null
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
      form.value.coverImageUrl = url
      form.value.coverImageId = mediaId || null
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

async function handleVideoUpload(file: File) {
  const maxSize = 200 * 1024 * 1024
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
      form.value.content += `\n<video src="${res.data.data.url}" controls style="max-width:100%"></video>\n`
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

/** AI 生成封面图 */
async function handleAiGenCover() {
  if (!form.value.content.trim()) {
    ElMessage.warning('请先输入项目内容')
    return
  }
  aiGenCoverLoading.value = true
  try {
    const res = await aiGenerateCoverApi({
      title: form.value.title || '未命名项目',
      content: form.value.content
    })
    if (res.data?.code === 200 && res.data.data) {
      form.value.coverImageUrl = res.data.data.url
      form.value.coverImageId = res.data.data.mediaId
      ElMessage.success('AI 封面生成完成')
    }
  } catch (err: any) {
    console.error('AI 生成封面失败:', err)
    if (err?.message && !err.message.includes('cancel')) {
      ElMessage.error('AI 生成封面失败: ' + (err.message || '未知错误'))
    }
  } finally {
    aiGenCoverLoading.value = false
  }
}

/** AI 润色项目内容 — 弹出选项框让用户选择 */
function handleAiPolish() {
  if (!form.value.content.trim()) {
    ElMessage.warning('请先输入项目内容')
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
  aiInsertImagePrompt.value = ''
  aiInsertImageAspectRatio.value = '16:9'
  aiInsertImageDialogVisible.value = true
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
      editorRef.value?.insert((_selectedText: string) => ({
        targetValue: mdImg,
        select: false,
      }))
      aiInsertImageDialogVisible.value = false
      ElMessage.success('AI 配图已插入')
    } else {
      ElMessage.error((res.data as any)?.message || '生成失败')
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

onMounted(() => {
  const id = route.params.id as string
  if (id) {
    isEdit.value = true
    projectId.value = id
    loadProject(id)
  }
})
</script>

<template>
  <div class="project-editor-page">
    <div class="page-header">
      <h3>{{ isEdit ? '编辑项目' : '新建项目' }}</h3>
      <div class="header-actions">
        <el-button @click="handleCancel">取消返回</el-button>
        <el-button type="primary" :loading="saving" @click="handleSave">保存</el-button>
      </div>
    </div>

    <el-card v-loading="loading" shadow="never">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="100px">
        <el-row :gutter="24">
          <el-col :span="12">
            <el-form-item label="项目标题" prop="title">
              <el-input v-model="form.title" placeholder="请输入项目标题" maxlength="255" @input="autoGenerateSlug" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="URL 标识" prop="slug">
              <el-input v-model="form.slug" placeholder="小写字母、数字和连字符" maxlength="255" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item label="项目简介">
          <el-input v-model="form.summary" type="textarea" :rows="2" placeholder="项目简介" maxlength="512" show-word-limit />
        </el-form-item>

        <!-- 封面图 -->
        <el-form-item label="封面图">
          <div class="cover-upload-wrapper">
            <div v-if="form.coverImageUrl" class="cover-preview">
              <img :src="form.coverImageUrl" alt="封面预览" />
              <el-button size="small" type="danger" circle class="cover-remove-btn" @click="form.coverImageUrl = ''; form.coverImageId = null">
                <el-icon><Close /></el-icon>
              </el-button>
            </div>
            <div class="cover-upload-actions">
              <el-upload
                :show-file-list="false"
                :auto-upload="false"
                accept="image/*"
                :on-change="handleCoverUpload"
              >
                <el-button size="small">上传封面图</el-button>
              </el-upload>
              <el-button size="small" @click="openCoverMediaSelector()">从媒体库选择</el-button>
              <el-button size="small" type="warning" :loading="aiGenCoverLoading" @click="handleAiGenCover">
                <el-icon><svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2l2.4 7.2h7.6l-6 4.8 2.4 7.2-6-4.8-6 4.8 2.4-7.2-6-4.8h7.6z"/></svg></el-icon> AI 生成
              </el-button>
            </div>
            <p class="cover-hint">建议尺寸：1920×1080（16:9），上传后自动裁剪为 16:9，支持 JPG/PNG/WebP</p>
          </div>
        </el-form-item>

        <el-form-item label="项目内容" class="editor-form-item">
          <div class="toolbar-row">
            <el-button size="small" @click="openMediaSelector('image')">从媒体库加载图片</el-button>
            <el-button size="small" @click="openMediaSelector('video')">从媒体库加载视频</el-button>
            <el-button size="small" @click="openMediaSelector('file')">从媒体库加载附件</el-button>
            <el-upload
              :show-file-list="false"
              :auto-upload="false"
              accept="video/mp4,video/webm,video/ogg"
              :on-change="(f: any) => handleVideoUpload(f.raw!)"
              class="toolbar-upload"
            >
              <el-button size="small">上传视频</el-button>
            </el-upload>
            <el-upload
              :show-file-list="false"
              :auto-upload="false"
              accept=".pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.zip,.rar,.7z,.txt,.csv,.json,.xml,.md"
              :on-change="(f: any) => handleFileUpload(f.raw!)"
              class="toolbar-upload"
            >
              <el-button size="small">上传附件</el-button>
            </el-upload>
            <el-divider direction="vertical" />
            <el-button size="small" plain @click="TencentInsertImage">
              <el-icon><svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="18" height="18" rx="2"/><circle cx="8.5" cy="8.5" r="1.5"/><path d="M21 15l-5-5L5 21"/></svg></el-icon> 插入 AI 图
            </el-button>
            <el-button size="small" type="warning" :loading="aiPolishLoading" @click="handleAiPolish">
              <el-icon><svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2l2.4 7.2h7.6l-6 4.8 2.4 7.2-6-4.8-6 4.8 2.4-7.2-6-4.8h7.6z"/></svg></el-icon> AI 润色
            </el-button>
          </div>
          <MarkdownEditor
            ref="editorRef"
            v-model="form.content"
            :min-height="500"
            placeholder="Markdown 格式内容..."
          />
        </el-form-item>

        <el-row :gutter="24">
          <el-col :span="12">
            <el-form-item label="技术栈">
              <el-input v-model="form.techStack" placeholder="逗号分隔，如: Vue3, TypeScript, Element Plus" />
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="排序">
              <el-input-number v-model="form.sortOrder" :min="0" style="width:100%" />
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="对外可见">
              <el-switch v-model="form.isVisible" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-divider content-position="left">链接配置</el-divider>

        <el-row :gutter="24">
          <el-col :span="8">
            <el-form-item label="项目地址">
              <el-input v-model="form.projectUrl" placeholder="https://..." />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="GitHub 地址">
              <el-input v-model="form.githubUrl" placeholder="https://github.com/..." />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="演示地址">
              <el-input v-model="form.demoUrl" placeholder="https://..." />
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
    </el-card>

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
.project-editor-page { padding: 0; }
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}
.toolbar-row {
  margin-bottom: 8px;
  display: flex;
  gap: 8px;
  justify-content: flex-end;
  align-items: center;
  flex-wrap: wrap;
}
.toolbar-upload {
  display: inline-flex;
  align-items: center;
}
.page-header h3 { margin: 0; font-size: 20px; }
.header-actions { display: flex; gap: 8px; }

.cover-upload-wrapper {
  display: flex;
  flex-direction: column;
  gap: 10px;
}
.cover-preview {
  position: relative;
  display: inline-block;
  max-width: 400px;
  border-radius: 6px;
  overflow: hidden;
  border: 1px solid #dcdfe6;
}
.cover-preview img {
  display: block;
  max-width: 100%;
  max-height: 200px;
  object-fit: contain;
}
.cover-remove-btn {
  position: absolute;
  top: 6px;
  right: 6px;
}
.cover-upload-actions {
  display: flex;
  gap: 8px;
}
.cover-hint {
  margin: 0;
  font-size: 12px;
  color: #909399;
}
</style>