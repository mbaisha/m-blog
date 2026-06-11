import request from '@/utils/request'
import type { ApiResponse, PagedResponse } from '@/types/api'
import type { ProjectListItem, ProjectDetail, ProjectQueryParams, CreateProjectRequest, UpdateProjectRequest } from '@/types/project'

/** 分页查询项目列表 */
export function getProjectListApi(params: ProjectQueryParams) {
  return request.get<ApiResponse<PagedResponse<ProjectListItem>>>('/admin/projects', { params })
}

/** 获取项目详情 */
export function getProjectApi(id: string) {
  return request.get<ApiResponse<ProjectDetail>>(`/admin/projects/${id}`)
}

/** 创建项目 */
export function createProjectApi(data: CreateProjectRequest) {
  return request.post<ApiResponse<ProjectDetail>>('/admin/projects', data)
}

/** 更新项目 */
export function updateProjectApi(id: string, data: UpdateProjectRequest) {
  return request.put<ApiResponse<ProjectDetail>>(`/admin/projects/${id}`, data)
}

/** 删除项目 */
export function deleteProjectApi(id: string) {
  return request.delete<ApiResponse>(`/admin/projects/${id}`)
}