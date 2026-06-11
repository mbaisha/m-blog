<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { getLlmConfigApi, saveLlmConfigApi, type LlmConfig } from '@/api/llm'

const loading = ref(false)
const saving = ref(false)

const form = ref<LlmConfig>({
  baseUrl: '',
  apiKey: '',
  model: '',
  mode: 'chat',
  availableModels: '[]',
  systemPrompt: '',
  maxTokens: 4096,
  temperature: 0.7,
  memoryEnabled: false,
  maxMemoryRounds: 10
})

const availableModelsStr = ref('')

async function load() {
  loading.value = true
  try {
    const res = await getLlmConfigApi()
    if (res.data?.code === 200 && res.data.data) {
      const d = res.data.data
      form.value = {
        baseUrl: d.baseUrl ?? '',
        apiKey: d.apiKey ?? '',
        model: d.model ?? '',
        mode: d.mode ?? 'chat',
        availableModels: d.availableModels ?? '[]',
        systemPrompt: d.systemPrompt ?? '',
        maxTokens: d.maxTokens ?? 4096,
        temperature: d.temperature ?? 0.7,
        memoryEnabled: d.memoryEnabled ?? false,
        maxMemoryRounds: d.maxMemoryRounds ?? 10
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
    // 解析可用模型列表
    const models = availableModelsStr.value
      .split('\n')
      .map(s => s.trim())
      .filter(s => s.length > 0)

    await saveLlmConfigApi({
      ...form.value,
      availableModels: JSON.stringify(models)
    })
    ElMessage.success('LLM 配置已保存')
  } finally {
    saving.value = false
  }
}

onMounted(load)
</script>

<template>
  <div class="llm-settings" v-loading="loading">
    <h2>大模型设置</h2>
    <p class="desc">配置 AI 大模型的 API 连接参数，支持 Chat Completions 和 Responses 两种模式。</p>

    <el-form :model="form" label-width="120px" class="settings-form">
      <el-form-item label="Base URL" required>
        <el-input v-model="form.baseUrl" placeholder="https://api.openai.com/v1" />
        <div class="tip">不以 /v1 结尾的地址会自动补全。</div>
      </el-form-item>

      <el-form-item label="API Key" required>
        <el-input v-model="form.apiKey" type="password" show-password placeholder="sk-..." />
      </el-form-item>

      <el-form-item label="默认模型" required>
        <el-input v-model="form.model" placeholder="gpt-4o" />
      </el-form-item>

      <el-form-item label="API 模式" required>
        <el-radio-group v-model="form.mode">
          <el-radio value="chat">Chat Completions（/chat/completions）</el-radio>
          <el-radio value="responses">Responses API（/responses）</el-radio>
        </el-radio-group>
      </el-form-item>

      <el-form-item label="可用模型列表">
        <el-input v-model="availableModelsStr" type="textarea" :rows="4" placeholder="每行一个模型名称" />
        <div class="tip">每行输入一个模型名称，用于在对话页面中切换模型。</div>
      </el-form-item>

      <el-form-item label="系统提示词">
        <el-input v-model="form.systemPrompt" type="textarea" :rows="3" placeholder="设定 AI 的角色和行为..." />
      </el-form-item>

      <el-form-item label="最大 Token">
        <el-input-number v-model="form.maxTokens" :min="1" :max="131072" :step="256" />
      </el-form-item>

      <el-form-item label="温度">
        <el-slider v-model="form.temperature" :min="0" :max="2" :step="0.1" show-input />
      </el-form-item>

      <el-divider content-position="left">记忆设置</el-divider>

      <el-form-item label="启用记忆">
        <el-switch v-model="form.memoryEnabled" />
        <span class="ml-2">开启后 AI 会记住当前会话的对话历史</span>
      </el-form-item>

      <el-form-item label="记忆轮数" v-if="form.memoryEnabled">
        <el-input-number v-model="form.maxMemoryRounds" :min="1" :max="50" />
        <span class="ml-2">保留最近 N 轮对话作为上下文</span>
      </el-form-item>

      <el-form-item>
        <el-button type="primary" @click="save" :loading="saving">保存配置</el-button>
      </el-form-item>
    </el-form>
  </div>
</template>

<style scoped>
.llm-settings { padding: 20px; max-width: 720px; }
h2 { margin: 0 0 8px 0; font-size: 20px; }
.desc { color: #909399; margin-bottom: 24px; }
.settings-form { max-width: 600px; }
.tip { color: #909399; font-size: 12px; margin-top: 4px; }
.ml-2 { margin-left: 8px; color: #666; font-size: 13px; }
</style>