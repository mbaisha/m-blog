<template>
  <el-dialog
    :model-value="visible"
    @update:model-value="$emit('update:visible', $event)"
    title="裁剪封面图"
    width="680px"
    :close-on-click-modal="false"
    :before-close="handleCancel"
    top="5vh"
  >
    <div class="cropper-container">
      <img ref="imgRef" :src="imageUrl" alt="裁剪预览" style="max-width:100%" />
    </div>
    <p class="cropper-hint">拖动选择裁剪区域，比例固定为 16:9</p>
    <template #footer>
      <el-button @click="handleCancel">取消</el-button>
      <el-button type="primary" :loading="uploading" @click="handleConfirm">确认裁剪</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, watch, nextTick } from 'vue'
import { ElMessage } from 'element-plus'
import Cropper from '@/vendor/cropper.esm.js'
import request from '@/utils/request'

const props = defineProps<{
  visible: boolean
  /** 上传文件（本地上传模式） */
  file: File | null
  /** 图片预览 URL（本地上传模式为 objectURL，媒体库模式为图片地址） */
  imageUrl: string
  /** 是否为从媒体库选择的图片（无需上传原文件，裁剪后上传即可） */
  fromMedia?: boolean
}>()

const emit = defineEmits<{
  (e: 'update:visible', val: boolean): void
  (e: 'success', url: string, id: string): void
}>()

const imgRef = ref<HTMLImageElement | null>(null)
let cropper: Cropper | null = null
const uploading = ref(false)

/** 检测图片是否为 16:9（±2% 容差） */
function checkRatio(width: number, height: number): boolean {
  const ratio = width / height
  const target = 16 / 9
  return Math.abs(ratio - target) / target < 0.02
}

/** 销毁 cropper 实例 */
function destroyCropper() {
  if (cropper) {
    cropper.destroy()
    cropper = null
  }
}

/** 初始化 cropper */
function initCropper() {
  destroyCropper()
  if (!imgRef.value) return

  const img = imgRef.value
  // 等待图片加载完成
  if (!img.complete) {
    img.onload = () => initCropper()
    return
  }

  // 如果已经是 16:9，自动确认，不弹裁切
  if (checkRatio(img.naturalWidth, img.naturalHeight)) {
    // 直接完成，不弹出裁剪
    handleSkipCrop()
    return
  }

  cropper = new Cropper(img, {
    aspectRatio: 16 / 9,
    viewMode: 1,
    dragMode: 'move',
    cropBoxMovable: true,
    cropBoxResizable: false,
    autoCropArea: 1,
    background: false,
    guides: true,
    center: true,
    highlight: false,
    movable: true,
    zoomable: true,
    rotatable: false,
    scalable: false,
  } as any)
}

/** 跳过裁剪（图片已是 16:9） */
async function handleSkipCrop() {
  if (props.fromMedia && props.imageUrl) {
    // 媒体库选择且已是 16:9 → 直接使用原图
    // 父组件会处理 URL/ID，这里只关闭弹窗
    uploading.value = true
    try {
      // 为了让父组件知道是"直接使用"的信号，触发 success 但 id 为空表示无需重新上传
      emit('success', props.imageUrl, '__skip_crop__')
      emit('update:visible', false)
    } finally {
      uploading.value = false
    }
  } else if (props.file) {
    // 本地上传且已是 16:9 → 直接上传原文件
    uploading.value = true
    try {
      const formData = new FormData()
      formData.append('file', props.file)
      const res = await request.post('/admin/upload/cover', formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })
      if (res.data.success && res.data.data) {
        emit('success', res.data.data.url, res.data.data.id)
        emit('update:visible', false)
      }
    } catch {
      ElMessage.error('封面上传失败')
    } finally {
      uploading.value = false
    }
  } else {
    emit('update:visible', false)
  }
}

watch(() => props.visible, (val) => {
  if (val) {
    nextTick(() => {
      setTimeout(initCropper, 100)
    })
  } else {
    destroyCropper()
  }
})

/** 上传裁剪后的图片 */
async function handleConfirm() {
  if (!cropper) return

  uploading.value = true
  try {
    // 从 cropper 获取裁剪后的 canvas
    const cropperAny = cropper as any
    const canvas = cropperAny.getCroppedCanvas({
      maxWidth: 1920,
      maxHeight: 1080,
      fillColor: '#fff',
      imageSmoothingEnabled: true,
      imageSmoothingQuality: 'high',
    })

    let blob: Blob | null

    if (props.fromMedia) {
      // 媒体库来源：canvas 直接转 blob 上传
      blob = await new Promise<Blob | null>((resolve) => {
        // @ts-ignore
        canvas.toBlob((b: Blob | null) => resolve(b), 'image/jpeg', 0.92)
      })
    } else {
      blob = await new Promise<Blob | null>((resolve) => {
        // @ts-ignore
        canvas.toBlob((b: Blob | null) => resolve(b), 'image/jpeg', 0.92)
      })
    }

    if (!blob) {
      throw new Error('图片裁剪失败')
    }

    const croppedFile = new File([blob], props.file?.name || 'cover.jpg', {
      type: 'image/jpeg',
    })

    const formData = new FormData()
    formData.append('file', croppedFile)
    const res = await request.post('/admin/upload/cover', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })

    if (res.data.success && res.data.data) {
      emit('success', res.data.data.url, res.data.data.id)
      emit('update:visible', false)
    } else {
      throw new Error(res.data.message || '上传失败')
    }
  } catch (e: any) {
    ElMessage.error(e.message || '裁剪上传失败')
  } finally {
    uploading.value = false
  }
}

function handleCancel() {
  destroyCropper()
  emit('update:visible', false)
}
</script>

<style>
.cropper-container {
  direction: ltr;
  font-size: 0;
  line-height: 0;
  position: relative;
  -ms-touch-action: none;
      touch-action: none;
  -webkit-touch-callout: none;
  -webkit-user-select: none;
     -moz-user-select: none;
      -ms-user-select: none;
          user-select: none;
}

.cropper-container img {
    backface-visibility: hidden;
    display: block;
    height: 100%;
    image-orientation: 0deg;
    max-height: none !important;
    max-width: none !important;
    min-height: 0 !important;
    min-width: 0 !important;
    width: 100%;
  }

.cropper-wrap-box,
.cropper-canvas,
.cropper-drag-box,
.cropper-crop-box,
.cropper-modal {
  bottom: 0;
  left: 0;
  position: absolute;
  right: 0;
  top: 0;
}

.cropper-wrap-box,
.cropper-canvas {
  overflow: hidden;
}

.cropper-drag-box {
  background-color: #fff;
  opacity: 0;
}

.cropper-modal {
  background-color: #000;
  opacity: 0.5;
}

.cropper-view-box {
  display: block;
  height: 100%;
  outline: 1px solid #39f;
  outline-color: rgba(51, 153, 255, 0.75);
  overflow: hidden;
  width: 100%;
}

.cropper-dashed {
  border: 0 dashed #eee;
  display: block;
  opacity: 0.5;
  position: absolute;
}

.cropper-dashed.dashed-h {
    border-bottom-width: 1px;
    border-top-width: 1px;
    height: calc(100% / 3);
    left: 0;
    top: calc(100% / 3);
    width: 100%;
  }

.cropper-dashed.dashed-v {
    border-left-width: 1px;
    border-right-width: 1px;
    height: 100%;
    left: calc(100% / 3);
    top: 0;
    width: calc(100% / 3);
  }

.cropper-center {
  display: block;
  height: 0;
  left: 50%;
  opacity: 0.75;
  position: absolute;
  top: 50%;
  width: 0;
}

.cropper-center::before,
  .cropper-center::after {
    background-color: #eee;
    content: ' ';
    display: block;
    position: absolute;
  }

.cropper-center::before {
    height: 1px;
    left: -3px;
    top: 0;
    width: 7px;
  }

.cropper-center::after {
    height: 7px;
    left: 0;
    top: -3px;
    width: 1px;
  }

.cropper-face,
.cropper-line,
.cropper-point {
  display: block;
  height: 100%;
  opacity: 0.1;
  position: absolute;
  width: 100%;
}

.cropper-face {
  background-color: #fff;
  left: 0;
  top: 0;
}

.cropper-line {
  background-color: #39f;
}

.cropper-line.line-e {
    cursor: ew-resize;
    right: -3px;
    top: 0;
    width: 5px;
  }

.cropper-line.line-n {
    cursor: ns-resize;
    height: 5px;
    left: 0;
    top: -3px;
  }

.cropper-line.line-w {
    cursor: ew-resize;
    left: -3px;
    top: 0;
    width: 5px;
  }

.cropper-line.line-s {
    bottom: -3px;
    cursor: ns-resize;
    height: 5px;
    left: 0;
  }

.cropper-point {
  background-color: #39f;
  height: 5px;
  opacity: 0.75;
  width: 5px;
}

.cropper-point.point-e {
    cursor: ew-resize;
    margin-top: -3px;
    right: -3px;
    top: 50%;
  }

.cropper-point.point-n {
    cursor: ns-resize;
    left: 50%;
    margin-left: -3px;
    top: -3px;
  }

.cropper-point.point-w {
    cursor: ew-resize;
    left: -3px;
    margin-top: -3px;
    top: 50%;
  }

.cropper-point.point-s {
    bottom: -3px;
    cursor: s-resize;
    left: 50%;
    margin-left: -3px;
  }

.cropper-point.point-ne {
    cursor: nesw-resize;
    right: -3px;
    top: -3px;
  }

.cropper-point.point-nw {
    cursor: nwse-resize;
    left: -3px;
    top: -3px;
  }

.cropper-point.point-sw {
    bottom: -3px;
    cursor: nesw-resize;
    left: -3px;
  }

.cropper-point.point-se {
    bottom: -3px;
    cursor: nwse-resize;
    height: 20px;
    opacity: 1;
    right: -3px;
    width: 20px;
  }

@media (min-width: 768px) {
.cropper-point.point-se {
      height: 15px;
      width: 15px;
  }
    }

@media (min-width: 992px) {
.cropper-point.point-se {
      height: 10px;
      width: 10px;
  }
    }

@media (min-width: 1200px) {
.cropper-point.point-se {
      height: 5px;
      opacity: 0.75;
      width: 5px;
  }
    }

.cropper-point.point-se::before {
    background-color: #39f;
    bottom: -50%;
    content: ' ';
    display: block;
    height: 200%;
    opacity: 0;
    position: absolute;
    right: -50%;
    width: 200%;
  }

.cropper-invisible {
  opacity: 0;
}

.cropper-bg {
  background-image: url('data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQAQMAAAAlPW0iAAAAA3NCSVQICAjb4U/gAAAABlBMVEXMzMz////TjRV2AAAACXBIWXMAAArrAAAK6wGCiw1aAAAAHHRFWHRTb2Z0d2FyZQBBZG9iZSBGaXJld29ya3MgQ1M26LyyjAAAABFJREFUCJlj+M/AgBVhF/0PAH6/D/HkDxOGAAAAAElFTkSuQmCC');
}

.cropper-hide {
  display: block;
  height: 0;
  position: absolute;
  width: 0;
}

.cropper-hidden {
  display: none !important;
}

.cropper-move {
  cursor: move;
}

.cropper-crop {
  cursor: crosshair;
}

.cropper-disabled .cropper-drag-box,
.cropper-disabled .cropper-face,
.cropper-disabled .cropper-line,
.cropper-disabled .cropper-point {
  cursor: not-allowed;
}
</style>

<style scoped>
.cropper-container {
  max-height: 60vh;
  display: flex;
  justify-content: center;
  background: #f0f0f0;
  border-radius: 4px;
  overflow: hidden;
}
.cropper-hint {
  margin: 12px 0 0;
  font-size: 13px;
  color: #909399;
  text-align: center;
}
</style>