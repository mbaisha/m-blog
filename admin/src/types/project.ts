/** 项目列表项 */
export interface ProjectListItem {
  id: string
  title: string
  slug: string
  summary: string | null
  coverImageUrl: string | null
  techStack: string[]
  projectUrl: string | null
  githubUrl: string | null
  demoUrl: string | null
  sortOrder: number
  isVisible: boolean
  createdAt: string
}

/** 项目详情 */
export interface ProjectDetail extends ProjectListItem {
  content: string | null
  coverImageId: string | null
  updatedAt: string
}

/** 创建项目请求 */
export interface CreateProjectRequest {
  title: string
  slug: string
  summary?: string | null
  content?: string | null
  coverImageId?: string | null
  techStack?: string[]
  projectUrl?: string | null
  githubUrl?: string | null
  demoUrl?: string | null
  sortOrder?: number
  isVisible?: boolean
}

/** 更新项目请求 */
export interface UpdateProjectRequest extends CreateProjectRequest {}

/** 项目查询参数 */
export interface ProjectQueryParams {
  page: number
  pageSize: number
  keyword?: string
  isVisible?: boolean
}