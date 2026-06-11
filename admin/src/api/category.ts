import request from '@/utils/request'
import type { ApiResponse } from '@/types/api'
import type {
  CategoryListItem,
  CategoryResponse,
  CreateCategoryRequest,
  UpdateCategoryRequest
} from '@/types/category'

/** 获取全部分类列表 */
export function getCategoryListApi() {
  return request.get<ApiResponse<CategoryListItem[]>>('/admin/categories')
}

/** 获取分类详情 */
export function getCategoryApi(id: string) {
  return request.get<ApiResponse<CategoryResponse>>(`/admin/categories/${id}`)
}

/** 创建分类 */
export function createCategoryApi(data: CreateCategoryRequest) {
  return request.post<ApiResponse<CategoryResponse>>('/admin/categories', data)
}

/** 更新分类 */
export function updateCategoryApi(id: string, data: UpdateCategoryRequest) {
  return request.put<ApiResponse<CategoryResponse>>(`/admin/categories/${id}`, data)
}

/** 删除分类 */
export function deleteCategoryApi(id: string) {
  return request.delete<ApiResponse>(`/admin/categories/${id}`)
}

/** 更新分类排序 */
export function updateCategorySortApi(id: string, sortOrder: number) {
  return request.patch<ApiResponse>(`/admin/categories/${id}/sort`, { sortOrder })
}

/** 获取分类平面列表（含层级信息，用于下拉选择） */
export function getCategoryFlatListApi() {
  return request.get<ApiResponse<CategoryListItem[]>>('/admin/categories/flat')
}