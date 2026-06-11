<script setup lang="ts">
import { ref, onMounted } from 'vue'
import type { Ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { MediaListItem } from '@/types/media'
import { getMediaListApi, deleteMediaApi, uploadImageApi, uploadVideoApi, uploadFileApi } from '@/api/media'

const loading = ref(false)
const uploadingImage = ref(false)
const uploadingVideo = ref(false)
const uploadingFile = ref(false)
const mediaList = ref<MediaListItem[]>([])
const pagination = ref({ page: 1, pageSize: 30, totalCount: 0, totalPages: 0 })
const typeFilter = ref('')
const keyword = ref('')

/** 加载列表 */
async function loadMedia() {
  loading.value = true
  try {
    const res = await getMediaListApi({
      page: pagination.value.page,
      pageSize: pagination.value.pageSize,
      type: typeFilter.value || undefined,
      keyword: keyword.value || undefined
    })
    const data = res.data.data
    mediaList.value = data?.items || []
    pagination.value = data || pagination.value
  } catch {
    // ignore
  } finally {
    loading.value = false
  }
}

/** 通用上传 */
async function handleUpload(file: File, uploadFn: (f: File) => Promise<any>, uploadingFlag: Ref<boolean>) {
  if (!file) return
  uploadingFlag.value = true
  try {
    await uploadFn(file)
    ElMessage.success('上传成功')
    await loadMedia()
  } catch {
    ElMessage.error('上传失败')
  } finally {
    uploadingFlag.value = false
  }
}

function handleUploadImage(file: File) {
  handleUpload(file, uploadImageApi, uploadingImage)
}

function handleUploadVideo(file: File) {
  handleUpload(file, uploadVideoApi, uploadingVideo)
}

function handleUploadFile(file: File) {
  handleUpload(file, uploadFileApi, uploadingFile)
}

/** 复制 URL */
function copyUrl(url: string) {
  navigator.clipboard.writeText(url).then(() => {
    ElMessage.success('已复制 URL')
  }).catch(() => {
    ElMessage.error('复制失败')
  })
}

/** 删除 */
async function handleDelete(id: string, filename: string) {
  try {
    await ElMessageBox.confirm(`确定要删除「${filename}」？`, '确认删除')
    await deleteMediaApi(id)
    ElMessage.success('文件已删除')
    await loadMedia()
  } catch {
    // ignore
  }
}

/** 格式化文件大小 */
function formatSize(bytes: number) {
  if (bytes < 1024) return bytes + ' B'
  if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB'
  return (bytes / (1024 * 1024)).toFixed(1) + ' MB'
}

/** 类型标签映射 */
const typeLabels: Record<string, string> = {
  image: '图片',
  video: '视频',
  file: '附件'
}
const typeColors: Record<string, string> = {
  image: 'success',
  video: 'warning',
  file: 'info'
}

onMounted(() => {
  loadMedia()
})
</script>

<template>
  <div class="media-page">
    <div class="page-header">
      <h3>媒体库</h3>
      <div class="header-actions">
        <el-upload
          :show-file-list="false"
          :auto-upload="false"
          accept="image/*"
          :on-change="(f: any) => handleUploadImage(f.raw as File)"
        >
          <el-button type="primary" :loading="uploadingImage">上传图片</el-button>
        </el-upload>
        <el-upload
          :show-file-list="false"
          :auto-upload="false"
          accept="video/mp4,video/webm,video/mov,video/avi,video/mkv"
          :on-change="(f: any) => handleUploadVideo(f.raw as File)"
        >
          <el-button type="success" :loading="uploadingVideo">上传视频</el-button>
        </el-upload>
        <el-upload
          :show-file-list="false"
          :auto-upload="false"
          accept=".pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.zip,.rar,.7z,.txt,.csv,.json,.xml,.md"
          :on-change="(f: any) => handleUploadFile(f.raw as File)"
        >
          <el-button type="warning" :loading="uploadingFile">上传附件</el-button>
        </el-upload>
      </div>
    </div>

    <!-- 筛选 -->
    <el-card shadow="never" class="filter-card">
      <el-form :inline="true">
        <el-form-item label="类型">
          <el-select v-model="typeFilter" clearable placeholder="全部" style="width: 120px" @change="loadMedia">
            <el-option label="图片" value="image" />
            <el-option label="视频" value="video" />
            <el-option label="附件" value="file" />
          </el-select>
        </el-form-item>
        <el-form-item label="关键词">
          <el-input v-model="keyword" placeholder="搜索文件名" clearable style="width: 200px" @keyup.enter="loadMedia" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="loadMedia">搜索</el-button>
          <el-button @click="typeFilter = ''; keyword = ''; loadMedia()">重置</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- 媒体网格 -->
    <el-card shadow="never" v-loading="loading">
      <template v-if="mediaList.length === 0">
        <el-empty description="暂无媒体文件" />
      </template>

      <div class="media-grid">
        <div v-for="item in mediaList" :key="item.id" class="media-item">
          <div class="media-preview">
            <img v-if="item.type === 'image'" :src="item.url" :alt="item.originalFilename || item.filename" />
            <div v-else-if="item.type === 'video'" class="video-icon">
              <el-icon :size="40"><VideoCamera /></el-icon>
            </div>
            <div v-else class="file-icon">
              <el-icon :size="40"><Document /></el-icon>
            </div>
          </div>
          <div class="media-info">
            <div class="media-filename" :title="item.originalFilename || item.filename">
              {{ item.originalFilename || item.filename }}
            </div>
            <div class="media-meta">
              <el-tag :type="typeColors[item.type] || 'info'" size="small">
                {{ typeLabels[item.type] || item.type }}
              </el-tag>
              <span class="media-size">{{ formatSize(item.sizeBytes) }}</span>
            </div>
          </div>
          <div class="media-actions">
            <el-button size="small" @click="copyUrl(item.url)">复制 URL</el-button>
            <el-button size="small" type="danger" @click="handleDelete(item.id, item.originalFilename || item.filename)">删除</el-button>
          </div>
        </div>
      </div>

      <div class="pagination-wrapper" v-if="pagination.totalPages > 1">
        <el-pagination
          v-model:current-page="pagination.page"
          :total="pagination.totalCount"
          :page-size="pagination.pageSize"
          layout="prev, pager, next"
          @current-change="loadMedia"
        />
      </div>
    </el-card>
  </div>
</template>

<style scoped>
.media-page {
  padding: 0;
}
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}
.page-header h3 {
  margin: 0;
  font-size: 20px;
}
.header-actions {
  display: flex;
  gap: 8px;
}
.filter-card {
  margin-bottom: 16px;
}
.media-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 16px;
}
.media-item {
  border: 1px solid #ebeef5;
  border-radius: 6px;
  overflow: hidden;
  transition: box-shadow 0.2s;
}
.media-item:hover {
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
}
.media-preview {
  aspect-ratio: 16 / 9;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f5f7fa;
  overflow: hidden;
}
.media-preview img {
  max-width: 100%;
  max-height: 100%;
  object-fit: contain;
}
.video-icon,
.file-icon {
  color: #909399;
}
.media-info {
  padding: 8px;
}
.media-filename {
  font-size: 13px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  margin-bottom: 4px;
}
.media-meta {
  display: flex;
  align-items: center;
  gap: 8px;
}
.media-size {
  font-size: 12px;
  color: #909399;
}
.media-actions {
  padding: 4px 8px 8px;
  display: flex;
  gap: 6px;
}
.pagination-wrapper {
  margin-top: 16px;
  display: flex;
  justify-content: center;
}
</style>