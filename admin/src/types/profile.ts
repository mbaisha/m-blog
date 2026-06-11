/** 个人页面模块 */
export interface ProfileSection {
  id: string
  sectionType: string
  title: string | null
  content: string | null
  sortOrder: number
  metadata: string
  isEnabled: boolean
  createdAt: string
  updatedAt: string
}

/** 保存个人页面模块请求 */
export interface SaveProfileSectionRequest {
  sectionType: string
  title?: string | null
  content?: string | null
  sortOrder: number
  metadata?: string
  isEnabled?: boolean
}

/** 个人页面模块类型选项 */
export const SECTION_TYPE_OPTIONS = [
  { label: 'Hero 区域', value: 'hero' },
  { label: '个人简介', value: 'intro' },
  { label: '技能特长', value: 'skills' },
  { label: '工作经历', value: 'experience' },
  { label: '教育背景', value: 'education' },
  { label: '联系方式', value: 'contact' },
  { label: '社交媒体', value: 'social' },
  { label: '自定义', value: 'custom' }
]