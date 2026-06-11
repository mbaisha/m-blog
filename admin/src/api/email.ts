import request from '@/utils/request'
import type { ApiResponse } from '@/types/api'
import type { EmailSetting, EmailTemplate } from '@/types/pages'

/** 获取邮件设置 */
export function getEmailSettingApi() {
  return request.get<ApiResponse<EmailSetting>>('/admin/email-settings')
}

/** 保存邮件设置 */
export function saveEmailSettingApi(data: Partial<EmailSetting>) {
  return request.put<ApiResponse<EmailSetting>>('/admin/email-settings', data)
}

/** 测试邮件发送 */
export function testEmailApi(testEmail: string) {
  return request.post<ApiResponse<null>>('/admin/email-settings/test', { testEmail })
}

/** 获取所有邮件模板 */
export function getEmailTemplatesApi() {
  return request.get<ApiResponse<EmailTemplate[]>>('/admin/email-templates')
}

/** 获取单个模板 */
export function getEmailTemplateApi(key: string) {
  return request.get<ApiResponse<EmailTemplate>>(`/admin/email-templates/${key}`)
}

/** 更新模板 */
export function updateEmailTemplateApi(key: string, data: { subject: string; htmlContent: string; description?: string }) {
  return request.put<ApiResponse<EmailTemplate>>(`/admin/email-templates/${key}`, data)
}

/** 重置模板 */
export function resetEmailTemplateApi(key: string) {
  return request.post<ApiResponse<EmailTemplate>>(`/admin/email-templates/${key}/reset`)
}