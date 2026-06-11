<script setup lang="ts">
import { ref, watch } from 'vue'
import { ElMessage } from 'element-plus'
import { MdEditor, NormalToolbar } from 'md-editor-v3'
import 'md-editor-v3/lib/style.css'
import request from '@/utils/request'
import { aiGenerateArticleImageApi, aiPolishArticleApi } from '@/api/article'
import MediaSelector from '@/components/MediaSelector.vue'

const props = defineProps<{
  modelValue: string
  placeholder?: string
  minHeight?: number
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void
}>()

const editorRef = ref()
const content = ref(props.modelValue)

// 同步外部 v-model 变化
watch(() => props.modelValue, (val) => {
  content.value = val
})

// ----- 媒体库选择 -----
const mediaDialogVisible = ref(false)
const mediaType = ref<'image' | 'video' | 'file'>('image')

function openMediaSelector(type: 'image' | 'video' | 'file') {
  mediaType.value = type
  mediaDialogVisible.value = true
}

/** 从媒体库插入到编辑器光标位置 */
function insertMediaUrl(url: string) {
  let markdown = ''
  if (mediaType.value === 'video') {
    markdown = `\n<video src="${url}" controls style="max-width:100%"></video>\n`
  } else if (mediaType.value === 'file') {
    markdown = `\n<a href="${url}" target="_blank" rel="noopener noreferrer">下载附件</a>\n`
  } else {
    markdown = `\n![图片](${url})\n`
  }

  mediaDialogVisible.value = false

  const view = editorRef.value?.getEditorView()
  if (view) {
    const pos = view.state.selection.main.head
    view.dispatch({ changes: { from: pos, insert: markdown } })
    view.focus()
  } else {
    content.value += markdown
    emit('update:modelValue', content.value)
  }
  ElMessage.success('已插入到编辑器')
}

// ----- 上传视频 -----
function handleVideoUpload() {
  const input = document.createElement('input')
  input.type = 'file'
  input.accept = 'video/mp4,video/webm,video/mov,video/avi,video/mkv'
  input.onchange = async (e: Event) => {
    const target = e.target as HTMLInputElement
    const file = target.files?.[0]
    if (!file) return

    if (file.size > 200 * 1024 * 1024) {
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
        const view = editorRef.value?.getEditorView()
        const md = `<video src="${res.data.data.url}" controls style="max-width:100%"></video>\n`
        if (view) {
          const pos = view.state.selection.main.head
          view.dispatch({ changes: { from: pos, insert: md } })
          view.focus()
        } else {
          content.value += md
          emit('update:modelValue', content.value)
        }
        ElMessage.success('视频已插入到编辑器中')
      }
    } catch {
      ElMessage.error('视频上传失败')
    }
  }
  input.click()
}

// ----- 上传附件 -----
function handleFileUpload() {
  const input = document.createElement('input')
  input.type = 'file'
  input.accept = '.pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.zip,.rar,.7z,.txt,.csv,.json,.xml,.md'
  input.onchange = async (e: Event) => {
    const target = e.target as HTMLInputElement
    const file = target.files?.[0]
    if (!file) return

    if (file.size > 100 * 1024 * 1024) {
      ElMessage.warning('附件不能超过 100MB')
      return
    }

    try {
      const formData = new FormData()
      formData.append('file', file)
      const res = await request.post('/admin/upload/file', formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })
      if (res.data.success && res.data.data?.url) {
        const view = editorRef.value?.getEditorView()
        const md = `<a href="${res.data.data.url}" target="_blank" rel="noopener noreferrer">下载附件</a>\n`
        if (view) {
          const pos = view.state.selection.main.head
          view.dispatch({ changes: { from: pos, insert: md } })
          view.focus()
        } else {
          content.value += md
          emit('update:modelValue', content.value)
        }
        ElMessage.success('附件已插入到编辑器中')
      }
    } catch {
      ElMessage.error('附件上传失败')
    }
  }
  input.click()
}

// ----- 编辑器内图片上传（通过工具栏 image 按钮触发） -----
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

// ----- AI 插入配图 -----
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

// ----- AI 润色 -----
const aiPolishDialogVisible = ref(false)
const aiPolishLoading = ref(false)
const aiPolishGenerateImages = ref(false)
const aiPolishAspectRatio = ref('16:9')

function handleAiPolish() {
  if (!content.value.trim()) {
    ElMessage.warning('请先输入内容')
    return
  }
  aiPolishGenerateImages.value = false
  aiPolishAspectRatio.value = '16:9'
  aiPolishDialogVisible.value = true
}

async function confirmAiPolish() {
  aiPolishDialogVisible.value = false
  aiPolishLoading.value = true
  try {
    const res = await aiPolishArticleApi({
      content: content.value,
      generateImages: aiPolishGenerateImages.value,
      aspectRatio: aiPolishGenerateImages.value ? aiPolishAspectRatio.value : undefined
    })
    if (res.data?.code === 200 && res.data.data) {
      content.value = res.data.data.content
      emit('update:modelValue', content.value)
      ElMessage.success(aiPolishGenerateImages.value ? 'AI 润色并配图完成' : 'AI 润色完成')
    } else {
      ElMessage.error((res.data as any)?.message || '润色失败')
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

// ----- v-model 同步 -----
function onInput(val: string) {
  content.value = val
  emit('update:modelValue', val)
}

// 暴露方法给父组件
defineExpose({
  getEditorView: () => editorRef.value?.getEditorView(),
  insert: (callback: (selectedText: string) => { targetValue: string; select: boolean }) => editorRef.value?.insert(callback),
  insertText: (text: string) => editorRef.value?.insertText(text),
})

/** 工具栏配置：数字 0-6 是 defToolbars 中 7 个自定义按钮的索引 */
const toolbars = ([
  'bold', 'italic', 'underline', 'strikeThrough', 'sub', 'sup',
  '-',
  'title', 'quote', 'unorderedList', 'orderedList', 'task',
  '-',
  'code', 'codeRow', 'link', 'image', 'table',
  '-',
  'revoke', 'next', 'save',
  '=',
  0, 1, 2, 3, 4, 5, 6, // 7 个自定义工具栏按钮
  '-',
  'pageFullscreen', 'fullscreen', 'preview',
  '-',
  'previewOnly', 'htmlPreview', 'catalog',
  '-',
  'katex', 'mermaid'
] as any)
</script>

<template>
  <div class="markdown-editor-wrapper" style="width:100%">
    <MdEditor
      ref="editorRef"
      :model-value="modelValue"
      :toolbars="toolbars"
      :placeholder="placeholder || '请输入 Markdown 内容...'"
      language="zh-CN"
      preview-theme="default"
      :style="{ minHeight: (minHeight || 500) + 'px', width: '100%' }"
      @on-upload-img="handleEditorUploadImg"
      @on-change="onInput"
    >
      <!-- 7 个自定义工具栏按钮 -->
      <template #defToolbars>
        <NormalToolbar title="上传视频" @onClick="handleVideoUpload">
          <svg class="md-editor-icon" aria-hidden="true" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <polygon points="23 7 16 12 23 17 23 7"/>
            <rect x="1" y="5" width="15" height="14" rx="2" ry="2"/>
          </svg>
        </NormalToolbar>
        <NormalToolbar title="上传附件" @onClick="handleFileUpload">
          <svg class="md-editor-icon" aria-hidden="true" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M21.44 11.05l-9.19 9.19a6 6 0 0 1-8.49-8.49l9.19-9.19a4 4 0 0 1 5.66 5.66l-9.2 9.19a2 2 0 0 1-2.83-2.83l8.49-8.48"/>
          </svg>
        </NormalToolbar>
        <NormalToolbar title="从媒体库加载图片" @onClick="() => openMediaSelector('image')">
          <svg class="md-editor-icon" aria-hidden="true" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <rect x="3" y="3" width="18" height="18" rx="2" ry="2"/>
            <circle cx="8.5" cy="8.5" r="1.5"/>
            <polyline points="21 15 16 10 5 21"/>
          </svg>
        </NormalToolbar>
        <NormalToolbar title="从媒体库加载视频" @onClick="() => openMediaSelector('video')">
          <svg class="md-editor-icon" aria-hidden="true" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <polygon points="23 7 16 12 23 17 23 7"/>
            <rect x="1" y="5" width="15" height="14" rx="2" ry="2"/>
          </svg>
        </NormalToolbar>
        <NormalToolbar title="从媒体库加载附件" @onClick="() => openMediaSelector('file')">
          <svg class="md-editor-icon" aria-hidden="true" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M13 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V9z"/>
            <polyline points="13 2 13 9 20 9"/>
          </svg>
        </NormalToolbar>
        <NormalToolbar title="插入 AI 图" @onClick="TencentInsertImage">
          <svg t="1781157395835" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="7423" width="24" height="24"><path d="M517.461333 223.573333a29.312 29.312 0 1 0 0 58.666667h239.872a9.813333 9.813333 0 0 1 9.813334 9.770667v547.541333a9.813333 9.813333 0 0 1-9.813334 9.813333H209.792a9.813333 9.813333 0 0 1-9.813333-9.813333v-311.936a29.312 29.312 0 1 0-58.666667 0v311.936c0 37.802667 30.677333 68.437333 68.48 68.437333H757.333333c37.802667 0 68.437333-30.634667 68.437334-68.437333V292.010667c0-37.802667-30.634667-68.437333-68.437334-68.437334h-239.872z" p-id="7424"></path><path d="M507.733333 485.12a29.312 29.312 0 0 1 48.64-1.28l146.645334 205.952a29.354667 29.354667 0 0 1-23.893334 46.378667H288a29.312 29.312 0 0 1-23.466667-46.933334l117.333334-157.013333a29.354667 29.354667 0 0 1 44.202666-3.2l32.853334 32.810667 48.768-76.714667z m26.069334 68.266667l-45.056 70.826666a29.312 29.312 0 0 1-45.482667 4.992l-34.730667-34.730666-62.037333 83.029333h275.712l-88.405333-124.16zM349.525333 236.16a256.512 256.512 0 0 1-47.786666 83.626667A280.192 280.192 0 0 1 247.466667 236.16h102.058666z m107.52 0V187.733333h-143.36L341.333333 179.2c-3.413333-14.336-12.970667-35.84-21.504-51.882667l-53.248 15.701334c6.485333 13.653333 12.629333 31.061333 16.042667 44.714666H141.994667v48.469334h53.930666c17.749333 46.08 39.594667 85.333333 67.925334 118.101333-34.474667 25.258667-77.141333 43.008-129.365334 54.613333 9.557333 11.605333 24.576 34.816 30.037334 46.762667 53.930667-15.018667 98.986667-36.522667 136.192-65.877333 35.157333 28.330667 77.824 49.493333 130.389333 63.146666 7.509333-13.653333 22.186667-35.498667 33.450667-46.421333-49.493333-10.922667-90.794667-29.013333-124.928-53.589333 27.989333-31.744 49.834667-70.314667 65.877333-116.736h51.541333z" p-id="7425"></path></svg>
        </NormalToolbar>
        <NormalToolbar title="AI 润色" @onClick="handleAiPolish">
          <svg t="1781157456205" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="9305" width="24" height="24"><path d="M546.56 610.688a41.6 41.6 0 0 1 58.88 0l256 256a41.6 41.6 0 0 1-58.88 58.88l-256-256a41.6 41.6 0 0 1 0-58.88zM850.56 58.88a41.6 41.6 0 0 1 18.624 55.808l-32 64a41.6 41.6 0 0 1-74.368-37.184l32-64a41.6 41.6 0 0 1 55.808-18.56zM969.536 294.528a41.6 41.6 0 0 1-23.808 53.824l-66.688 25.792a41.6 41.6 0 1 1-30.08-77.568l66.752-25.792a41.6 41.6 0 0 1 53.824 23.744z" fill="#333333" p-id="9306"></path><path d="M378.688 221.952L176.704 184.96l-6.592-0.768a48 48 0 0 0-49.28 56.64l37.056 202.048L60.16 623.488l-2.688 6.016a48 48 0 0 0 38.592 64.448l203.648 27.072 141.568 148.8 4.864 4.48a48 48 0 0 0 73.216-16.832l88.768-185.344 185.216-88.64 5.76-3.2a48 48 0 0 0 6.592-74.88l-148.736-141.632-27.2-203.52a48 48 0 0 0-70.4-35.904L378.688 221.952z m175.36-0.256l22.08 165.376 1.28 6.4a48 48 0 0 0 13.184 22.016l120.832 114.944-150.464 72.128-5.696 3.2a48 48 0 0 0-16.832 19.264l-72.064 150.464L351.36 654.72l-4.8-4.416a47.872 47.872 0 0 0-23.68-10.048l-165.248-22.144 79.36-146.56 2.688-6.016a48 48 0 0 0 2.304-25.536l-30.08-164.096 163.968 30.208a48 48 0 0 0 31.488-4.992l146.688-79.36z" fill="#333333" p-id="9307"></path></svg>
        </NormalToolbar>
      </template>
    </MdEditor>

    <!-- 媒体选择对话框 -->
    <MediaSelector
      v-model:visible="mediaDialogVisible"
      :type="mediaType"
      @insert="insertMediaUrl"
    />

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
  </div>
</template>
