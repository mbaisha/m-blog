<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue'
import { ElMessage } from 'element-plus'
import { getMediaListApi, uploadImageApi, uploadVideoApi, uploadFileApi } from '@/api/media'
import type { MediaListItem } from '@/types/media'

const props = defineProps<{
  visible: boolean
  type: 'image' | 'video' | 'file'
}>()

const emit = defineEmits<{
  (e: 'update:visible', val: boolean): void
  (e: 'insert', url: string, id?: string): void
}>()

/** 媒体列表 */
const mediaList = ref<MediaListItem[]>([])
const loading = ref(false)
const uploading = ref(false)

/** 分页 */
const page = ref(1)
const pageSize = ref(9)
const totalCount = ref(0)
const totalPages = ref(0)

/** 搜索关键词 */
const keyword = ref('')
let searchTimer: ReturnType<typeof setTimeout> | null = null

function handleSearchInput() {
  if (searchTimer) clearTimeout(searchTimer)
  searchTimer = setTimeout(() => {
    page.value = 1
    loadMediaList()
  }, 300)
}

/** 清除搜索 */
function clearSearch() {
  keyword.value = ''
  page.value = 1
  loadMediaList()
}

/** 上传用 */
const uploadRef = ref()

/** 对话框标题 */
const dialogTitle = computed(() => {
  const map: Record<string, string> = { image: '从媒体库选择图片', video: '从媒体库选择视频', file: '从媒体库选择附件' }
  return map[props.type] || '从媒体库选择'
})

/** 上传按钮文字 */
const uploadBtnText = computed(() => {
  const map: Record<string, string> = { image: '上传图片', video: '上传视频', file: '上传附件' }
  return map[props.type] || '上传'
})

/** 上传大小限制（文字） */
const sizeLimitText = computed(() => {
  const map: Record<string, string> = { image: '10MB', video: '200MB', file: '100MB' }
  return map[props.type] || ''
})

/** 上传大小限制（字节） */
const sizeLimitBytes = computed(() => {
  const map: Record<string, number> = { image: 10 * 1024 * 1024, video: 200 * 1024 * 1024, file: 100 * 1024 * 1024 }
  return map[props.type] || 10 * 1024 * 1024
})

/** 文件类型 accept */
const acceptAttr = computed(() => {
  switch (props.type) {
    case 'image': return 'image/*'
    case 'video': return 'video/mp4,video/webm,video/mov,video/avi,video/mkv'
    case 'file': return '.pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.zip,.rar,.7z,.txt,.csv,.json,.xml,.md,.png,.jpg,.jpeg,.gif,.webp'
    default: return ''
  }
})

/** 文件图标映射 */
const fileExtensionIcon: Record<string, string> = {
  pdf: '📄', doc: '📝', docx: '📝',
  xls: '📊', xlsx: '📊',
  ppt: '📑', pptx: '📑',
  zip: '📦', rar: '📦', '7z': '📦',
  txt: '📃', csv: '📋', json: '📋', xml: '📋', md: '📋'
}

function getFileIcon(url: string, filename: string): string {
  const ext = filename?.split('.').pop()?.toLowerCase() || url?.split('.').pop()?.toLowerCase() || ''
  return fileExtensionIcon[ext] || '📎'
}

/** 加载媒体列表 */
async function loadMediaList() {
  loading.value = true
  try {
    const res = await getMediaListApi({
      page: page.value,
      pageSize: pageSize.value,
      type: props.type,
      keyword: keyword.value || undefined
    })
    const data = res.data.data
    if (data) {
      mediaList.value = data.items || []
      totalCount.value = data.totalCount
      totalPages.value = data.totalPages
    } else {
      mediaList.value = []
    }
  } catch {
    mediaList.value = []
  } finally {
    loading.value = false
  }
}

/** 翻页 */
function handlePageChange(p: number) {
  page.value = p
  loadMediaList()
}

/** 上传文件（支持多文件） */
async function handleUpload(files: File | File[]) {
  const fileList = Array.isArray(files) ? files : [files]
  if (fileList.length === 0) return

  const maxSize = sizeLimitBytes.value
  let uploadFn: (file: File) => Promise<any>

  switch (props.type) {
    case 'image': uploadFn = uploadImageApi; break
    case 'video': uploadFn = uploadVideoApi; break
    case 'file': uploadFn = uploadFileApi; break
    default: return
  }

  uploading.value = true
  let successCount = 0

  for (const file of fileList) {
    if (file.size > maxSize) {
      ElMessage.warning(`${file.name} 超出大小限制 (${sizeLimitText.value})`)
      continue
    }
    try {
      await uploadFn(file)
      successCount++
    } catch {
      ElMessage.error(`${file.name} 上传失败`)
    }
  }

  uploading.value = false

  if (successCount > 0) {
    ElMessage.success(`成功上传 ${successCount} 个文件`)
    page.value = 1
    await loadMediaList()
  }
}

/** 选择文件 */
function handleFileChange() {
  if (!uploadRef.value) return
  const files: File[] = uploadRef.value.files
  if (files && files.length > 0) {
    handleUpload(Array.from(files))
    uploadRef.value.value = ''
  }
}

/** 插入媒体 */
function handleInsert(item: MediaListItem) {
  emit('insert', item.url, item.id)
  emit('update:visible', false)
}

/** 监听对话框打开 */
watch(() => props.visible, (val) => {
  if (val) {
    page.value = 1
    loadMediaList()
  }
})

onMounted(() => {
  if (props.visible) loadMediaList()
})
</script>

<template>
  <el-dialog
    :model-value="visible"
    @update:model-value="(v: boolean) => emit('update:visible', v)"
    :title="dialogTitle"
    width="800px"
    top="5vh"
  >
    <!-- 上传 + 搜索区域 -->
    <div class="media-toolbar">
      <div class="media-upload-bar">
        <input
          ref="uploadRef"
          type="file"
          :accept="acceptAttr"
          multiple
          style="display:none"
          @change="handleFileChange"
        />
        <el-button
          type="primary"
          :loading="uploading"
          @click="uploadRef?.click()"
        >
          <el-icon><Upload /></el-icon>
          {{ uploadBtnText }}
        </el-button>
        <span class="upload-hint">支持多文件同时上传，大小限制 {{ sizeLimitText }}</span>
      </div>
      <div class="media-search-bar">
        <el-input
          v-model="keyword"
          placeholder="搜索文件名..."
          clearable
          size="default"
          style="width:220px"
          @input="handleSearchInput"
          @clear="clearSearch"
        >
          <template #prefix>
            <el-icon><Search /></el-icon>
          </template>
        </el-input>
      </div>
    </div>

    <!-- 媒体列表 -->
    <div v-loading="loading" class="media-list-wrap">
      <el-empty v-if="!loading && mediaList.length === 0" description="暂无媒体文件" />
      <el-row v-else :gutter="12">
        <el-col
          v-for="item in mediaList"
          :key="item.id"
          :span="8"
          style="margin-bottom:12px"
        >
          <el-card
            shadow="hover"
            :body-style="{ padding: '8px', cursor: 'pointer' }"
            @click="handleInsert(item)"
          >
            <template v-if="type === 'image'">
              <div class="media-image-wrap">
                <el-image
                  :src="item.thumbnailUrl || item.url"
                  fit="scale-down"
                />
              </div>
            </template>
            <template v-else-if="type === 'video'">
              <div class="media-thumb-placeholder">
                <svg viewBox="0 0 1024 1024" width="48" height="48" fill="#909399">
                  <path d="M512 64C264.6 64 64 264.6 64 512s200.6 448 448 448 448-200.6 448-448S759.4 64 512 64zm0 820c-205.4 0-372-166.6-372-372s166.6-372 372-372 372 166.6 372 372-166.6 372-372 372z"/>
                  <path d="M719.4 499.1l-296.1-215A15.9 15.9 0 0 0 398 297v430c0 13.1 14.8 20.5 25.3 12.9l296.1-215a15.9 15.9 0 0 0 0-25.8z"/>
                </svg>
              </div>
            </template>
            <template v-else>
              <div class="media-thumb-placeholder file-thumb">
                <span class="file-icon">{{ getFileIcon(item.url, item.originalFilename || item.filename || '') }}</span>
              </div>
            </template>
            <div class="media-filename" :title="item.originalFilename || item.filename || item.url?.split('/').pop()">
              <el-tag size="small" class="ext-tag">
                {{ (item.originalFilename || item.filename || item.url || '').split('.').pop()?.toUpperCase() || '?' }}
              </el-tag>
              {{ item.originalFilename || item.filename || item.url?.split('/').pop() }}
            </div>
          </el-card>
        </el-col>
      </el-row>
    </div>

    <!-- 分页 -->
    <div v-if="totalPages > 1" class="media-pagination">
      <el-pagination
        v-model:current-page="page"
        :page-size="pageSize"
        :total="totalCount"
        layout="prev, pager, next, total"
        @current-change="handlePageChange"
      />
    </div>

    <template #footer>
      <el-button @click="emit('update:visible', false)">取消</el-button>
    </template>
  </el-dialog>
</template>

<style scoped>
.media-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
  gap: 12px;
  flex-wrap: wrap;
}
.media-upload-bar {
  display: flex;
  align-items: center;
  gap: 12px;
}
.upload-hint {
  font-size: 12px;
  color: #909399;
}
.media-list-wrap {
  min-height: 200px;
}
.media-thumb-placeholder {
  width: 100%;
  aspect-ratio: 16 / 9;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f5f7fa;
  border-radius: 4px;
}
.media-thumb-placeholder.file-thumb {
  background: #fafafa;
}
.file-icon {
  font-size: 36px;
  line-height: 1;
}
.media-filename {
  margin-top: 6px;
  font-size: 12px;
  color: #606266;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.media-image-wrap {
  width: 100%;
  aspect-ratio: 16 / 9;
  background: #f5f7fa;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  border-radius: 4px;
}
.media-image-wrap .el-image {
  width: 100%;
  height: 100%;
}
.ext-tag {
  margin-right: 4px;
}
.media-pagination {
  display: flex;
  justify-content: center;
  margin-top: 16px;
}
</style>