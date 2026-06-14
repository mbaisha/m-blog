<script setup lang="ts">
import { ref, nextTick, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { getLlmConfigApi, llmChatApi } from '@/api/llm'
import { runtimeConfig } from '@/utils/runtimeConfig'

interface Message {
  role: 'user' | 'assistant'
  content: string
}

const loading = ref(false)
const sending = ref(false)
const streamMode = ref(false)
const messages = ref<Message[]>([])
const inputText = ref('')
const sessionId = ref<string | undefined>(undefined)
const chatContainer = ref<HTMLElement | null>(null)
const configLoaded = ref(false)
const streamAbort = ref<AbortController | null>(null)

onMounted(async () => {
  try {
    const res = await getLlmConfigApi()
    if (res.data?.code === 200 && res.data.data) {
      configLoaded.value = true
    }
  } catch {
    // 配置未加载，仍可使用但会提示
  }
})

async function send() {
  const text = inputText.value.trim()
  if (!text || sending.value) return

  messages.value.push({ role: 'user', content: text })
  inputText.value = ''
  await scrollToBottom()

  const currentMessages = [...messages.value]
  const assistantMsg: Message = { role: 'assistant', content: '' }
  messages.value.push(assistantMsg)

  sending.value = true
  const assistantIndex = messages.value.length - 1

  try {
    if (streamMode.value) {
      await sendStream(text, assistantIndex)
    } else {
      const res = await llmChatApi({ message: text, sessionId: sessionId.value, stream: false })
      if (res.data?.code === 200 && res.data.data) {
        messages.value[assistantIndex].content = res.data.data.content
        if (res.data.data.sessionId) sessionId.value = res.data.data.sessionId
      }
    }
  } catch (e: any) {
    const err = e?.response?.data?.message || e?.message || '请求失败'
    messages.value[assistantIndex].content = `错误: ${err}`
  } finally {
    sending.value = false
    await scrollToBottom()
  }
}

async function sendStream(text: string, assistantIndex: number) {
  const token = localStorage.getItem('accessToken')
  const controller = new AbortController()
  streamAbort.value = controller

  const response = await fetch(`${runtimeConfig.API_BASE_URL}/admin/llm/chat`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({ message: text, sessionId: sessionId.value, stream: true }),
    signal: controller.signal
  })

  if (!response.ok) {
    const errText = await response.text()
    throw new Error(errText || `HTTP ${response.status}`)
  }

  const reader = response.body?.getReader()
  if (!reader) throw new Error('不支持流式读取')

  const decoder = new TextDecoder()
  while (true) {
    const { done, value } = await reader.read()
    if (done) break
    const text = decoder.decode(value, { stream: true })
    const lines = text.split('\n')
    for (const line of lines) {
      if (!line.startsWith('data: ')) continue
      const data = line.slice(6)
      if (data === '[DONE]') break
      messages.value[assistantIndex].content += data
    }
    await nextTick()
    await scrollToBottom()
  }
  streamAbort.value = null
}

function stopStream() {
  streamAbort.value?.abort()
  streamAbort.value = null
  sending.value = false
}

function clearChat() {
  messages.value = []
  sessionId.value = undefined
}

function handleKeydown(e: KeyboardEvent) {
  if (e.key === 'Enter' && !e.shiftKey) {
    e.preventDefault()
    send()
  }
}

async function scrollToBottom() {
  await nextTick()
  if (chatContainer.value) {
    chatContainer.value.scrollTop = chatContainer.value.scrollHeight
  }
}
</script>

<template>
  <div class="llm-chat">
    <h2>AI 对话</h2>

    <div class="chat-card">
      <div class="chat-toolbar">
        <label class="stream-toggle">
          <input type="checkbox" v-model="streamMode" />
          流式输出
        </label>
        <el-button size="small" text @click="clearChat" :disabled="sending">清空对话</el-button>
      </div>

      <div class="chat-messages" ref="chatContainer">
        <div v-if="messages.length === 0" class="empty-hint">
          输入消息开始与 AI 对话
        </div>
        <div v-for="(msg, i) in messages" :key="i" :class="['chat-msg', msg.role]">
          <div class="chat-role">{{ msg.role === 'user' ? '你' : 'AI' }}</div>
          <div class="chat-content" v-text="msg.content || (msg.role === 'assistant' && sending ? '思考中...' : '')"></div>
        </div>
      </div>

      <div class="chat-input-area">
        <el-input
          v-model="inputText"
          type="textarea"
          :rows="3"
          placeholder="输入消息..."
          @keydown="handleKeydown"
          :disabled="sending"
        />
        <div class="chat-actions">
          <el-button type="primary" @click="send" :loading="sending && !streamMode" :disabled="!inputText.trim()">
            发送
          </el-button>
          <el-button v-if="sending && streamMode" type="warning" @click="stopStream">停止</el-button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.llm-chat { padding: 20px; height: calc(100vh - 80px); display: flex; flex-direction: column; }
h2 { margin: 0 0 12px 0; font-size: 20px; }
.chat-card { flex: 1; display: flex; flex-direction: column; border: 1px solid #e4e7ed; border-radius: 8px; overflow: hidden; }
.chat-toolbar { display: flex; align-items: center; justify-content: space-between; padding: 8px 16px; border-bottom: 1px solid #eee; background: #fafafa; }
.stream-toggle { display: flex; align-items: center; gap: 6px; font-size: 13px; color: #666; cursor: pointer; }
.chat-messages { flex: 1; overflow-y: auto; padding: 16px; }
.empty-hint { text-align: center; color: #999; padding-top: 80px; }
.chat-msg { margin-bottom: 16px; }
.chat-role { font-size: 12px; font-weight: 600; color: #909399; margin-bottom: 4px; }
.chat-msg.user .chat-role { color: #409eff; }
.chat-content { 
  background: #f0f0f0; padding: 10px 14px; border-radius: 8px; 
  line-height: 1.6; white-space: pre-wrap; word-break: break-word; 
}
.chat-msg.user .chat-content { background: #ecf5ff; }
.chat-input-area { padding: 12px 16px; border-top: 1px solid #eee; }
.chat-actions { margin-top: 8px; display: flex; gap: 8px; }
</style>