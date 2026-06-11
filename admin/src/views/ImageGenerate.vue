<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { getImageGenConfigApi, generateImageApi } from '@/api/imageGen'

const loading = ref(false)
const generating = ref(false)
const images = ref<string[]>([])
const prompt = ref('')
const model = ref('')
const size = ref('')
const quality = ref('')
const n = ref(1)

const sizeOptions = [
  '1664x2496', '2496x1664',
  '1760x2368', '2368x1760',
  '1824x2272', '2272x1824',
  '2048x2048',
  '2752x1536', '1536x2752',
  '3072x1376', '1344x3136'
]
const qualityOptions = [
  { label: '标准 (standard)', value: 'standard' },
  { label: '高清 (hd)', value: 'hd' }
]

onMounted(async () => {
  loading.value = true
  try {
    const res = await getImageGenConfigApi()
    if (res.data?.code === 200 && res.data.data) {
      const d = res.data.data
      size.value = d.defaultSize || '2048x2048'
      quality.value = d.defaultQuality || 'standard'
      n.value = d.defaultN || 1
    }
  } catch { /* ignore */ }
  loading.value = false
})

async function generate() {
  if (!prompt.value.trim()) {
    ElMessage.warning('请输入提示词')
    return
  }

  generating.value = true
  images.value = []
  try {
    const res = await generateImageApi({
      prompt: prompt.value.trim(),
      model: model.value || undefined,
      size: size.value || undefined,
      quality: quality.value || undefined,
      n: n.value
    })
    if (res.data?.code === 200 && res.data.data) {
      images.value = res.data.data.images || []
      ElMessage.success('生成成功')
    } else {
      ElMessage.error(res.data?.message || '生成失败')
    }
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || e?.message || '请求失败')
  } finally {
    generating.value = false
  }
}
</script>

<template>
  <div class="image-gen-page" v-loading="loading">
    <h2>文生图</h2>
    <p class="desc">通过 AI 模型根据文字描述生成图片。</p>

    <div class="gen-form">
      <el-form label-width="80px">
        <el-form-item label="提示词" required>
          <el-input v-model="prompt" type="textarea" :rows="3" placeholder="描述你想要的图片..." />
        </el-form-item>

        <el-form-item label="模型">
          <el-input v-model="model" placeholder="留空使用默认模型" />
        </el-form-item>

        <el-form-item label="尺寸">
          <el-select v-model="size">
            <el-option v-for="s in sizeOptions" :key="s" :label="s" :value="s" />
          </el-select>
        </el-form-item>

        <el-form-item label="质量">
          <el-select v-model="quality">
            <el-option v-for="q in qualityOptions" :key="q.value" :label="q.label" :value="q.value" />
          </el-select>
        </el-form-item>

        <el-form-item label="数量">
          <el-input-number v-model="n" :min="1" :max="10" />
        </el-form-item>

        <el-form-item>
          <el-button type="primary" @click="generate" :loading="generating">
            {{ generating ? '生成中...' : '生成图片' }}
          </el-button>
        </el-form-item>
      </el-form>
    </div>

    <div v-if="images.length > 0" class="gen-result">
      <h3>生成结果</h3>
      <div class="image-grid">
        <div v-for="(url, i) in images" :key="i" class="image-item">
          <img :src="url" :alt="`生成图片 ${i + 1}`" />
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.image-gen-page { padding: 20px; max-width: 900px; }
h2 { margin: 0 0 8px 0; font-size: 20px; }
.desc { color: #909399; margin-bottom: 24px; }
.gen-form { max-width: 500px; }
.gen-result { margin-top: 32px; }
.gen-result h3 { font-size: 16px; margin-bottom: 12px; }
.image-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 16px; }
.image-item { border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }
.image-item img { width: 100%; display: block; }
</style>