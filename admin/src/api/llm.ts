import request from '@/utils/request'
import type { ApiResponse } from '@/types/api'

export interface LlmConfig {
  baseUrl: string
  apiKey: string
  model: string
  mode: string
  availableModels: string
  systemPrompt: string
  maxTokens: number
  temperature: number
  memoryEnabled: boolean
  maxMemoryRounds: number
}

export interface SaveLlmConfigRequest {
  baseUrl: string
  apiKey: string
  model: string
  mode: string
  availableModels: string
  systemPrompt?: string
  maxTokens: number
  temperature: number
  memoryEnabled: boolean
  maxMemoryRounds: number
}

export interface LlmChatRequest {
  message: string
  sessionId?: string
  stream: boolean
}

/** 获取 LLM 配置 */
export function getLlmConfigApi() {
  return request.get<ApiResponse<LlmConfig>>('/admin/llm/config')
}

/** 保存 LLM 配置 */
export function saveLlmConfigApi(data: SaveLlmConfigRequest) {
  return request.put<ApiResponse>('/admin/llm/config', data)
}

/** LLM 非流式对话 */
export function llmChatApi(data: LlmChatRequest) {
  return request.post<ApiResponse<{ content: string; sessionId?: string }>>('/admin/llm/chat', data)
}