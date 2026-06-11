/** 友情链接列表项 */
export interface FriendListItem {
  id: string
  name: string
  url: string
  description: string | null
  logoImageUrl: string | null
  sortOrder: number
  isVisible: boolean
  createdAt: string
}

/** 创建友情链接请求 */
export interface CreateFriendRequest {
  name: string
  url: string
  description?: string | null
  logoImageId?: string | null
  sortOrder?: number
  isVisible?: boolean
}

/** 更新友情链接请求 */
export interface UpdateFriendRequest extends CreateFriendRequest {}