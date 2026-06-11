<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { getImageGenConfigApi, saveImageGenConfigApi, type ImageGenConfig } from '@/api/imageGen'

const loading = ref(false)
const saving = ref(false)

const form = ref<ImageGenConfig>({
  baseUrl: '',
  apiKey: '',
  model: '',
  availableModels: '[]',
  defaultSize: '2048x2048',
  defaultQuality: 'standard',
  defaultN: 1
})

const availableModelsStr = ref('')

async function load() {
  loading.value = true
  try {
    const res = await getImageGenConfigApi()
    if (res.data?.code === 200 && res.data.data) {
      const d = res.data.data
      form.value = {
        baseUrl: d.baseUrl ?? '',
        apiKey: d.apiKey ?? '',
        model: d.model ?? '',
        availableModels: d.availableModels ?? '[]',
        defaultSize: d.defaultSize ?? '2048x2048',
        defaultQuality: d.defaultQuality ?? 'standard',
        defaultN: d.defaultN ?? 1
      }
      try {
        const arr = JSON.parse(d.availableModels || '[]')
        availableModelsStr.value = Array.isArray(arr) ? arr.join('\n') : ''
      } catch {
        availableModelsStr.value = ''
      }
    }
  } finally {
    loading.value = false
  }
}

async function save() {
  saving.value = true
  try {
    const models = availableModelsStr.value
      .split('\n')
      .map(s => s.trim())
      .filter(s => s.length > 0)

    await saveImageGenConfigApi({
      ...form.value,
      availableModels: JSON.stringify(models)
    })
    ElMessage.success('文生图配置已保存')
  } finally {
    saving.value = false
  }
}

onMounted(load)
</script>

<template>
  <div class="image-gen-settings" v-loading="loading">
    <h2>文生图模型设置</h2>
    <p class="desc">配置文生图 API 连接参数，接口路径为 /images/generations。</p>

    <el-form :model="form" label-width="120px" class="settings-form">
      <el-form-item label="Base URL" required>
        <el-input v-model="form.baseUrl" placeholder="https://api.openai.com/v1" />
        <div class="tip">不以 /v1 结尾的地址会自动补全。</div>
      </el-form-item>

      <el-form-item label="API Key" required>
        <el-input v-model="form.apiKey" type="password" show-password placeholder="sk-..." />
      </el-form-item>

      <el-form-item label="默认模型" required>
        <el-input v-model="form.model" placeholder="dall-e-3" />
      </el-form-item>

      <el-form-item label="可用模型列表">
        <el-input v-model="availableModelsStr" type="textarea" :rows="4" placeholder="每行一个模型名称" />
        <div class="tip">每行输入一个模型名称。</div>
      </el-form-item>

      <el-divider content-position="left">默认参数</el-divider>

      <el-form-item label="默认尺寸">
        <el-select v-model="form.defaultSize">
          <el-option label="1664x2496" value="1664x2496" />
          <el-option label="2496x1664" value="2496x1664" />
          <el-option label="1760x2368" value="1760x2368" />
          <el-option label="2368x1760" value="2368x1760" />
          <el-option label="1824x2272" value="1824x2272" />
          <el-option label="2272x1824" value="2272x1824" />
          <el-option label="2048x2048" value="2048x2048" />
          <el-option label="2752x1536" value="2752x1536" />
          <el-option label="1536x2752" value="1536x2752" />
          <el-option label="3072x1376" value="3072x1376" />
          <el-option label="1344x3136" value="1344x3136" />
        </el-select>
      </el-form-item>

      <el-form-item label="默认质量">
        <el-select v-model="form.defaultQuality">
          <el-option label="标准 (standard)" value="standard" />
          <el-option label="高清 (hd)" value="hd" />
        </el-select>
      </el-form-item>

      <el-form-item label="默认数量">
        <el-input-number v-model="form.defaultN" :min="1" :max="10" />
      </el-form-item>

      <el-form-item>
        <el-button type="primary" @click="save" :loading="saving">保存配置</el-button>
      </el-form-item>
    </el-form>
  </div>
</template>

<style scoped>
.image-gen-settings { padding: 20px; max-width: 720px; }
h2 { margin: 0 0 8px 0; font-size: 20px; }
.desc { color: #909399; margin-bottom: 24px; }
.settings-form { max-width: 600px; }
.tip { color: #909399; font-size: 12px; margin-top: 4px; }
</style>