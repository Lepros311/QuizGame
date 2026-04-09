import { apiClient } from './client'
import type { NotificationDto } from '../types/api'

export async function fetchNotifications(): Promise<NotificationDto[]> {
  const { data } = await apiClient.get<NotificationDto[]>('/api/notification')
  return data
}

export async function markNotificationRead(id: number): Promise<void> {
  await apiClient.patch(`/api/notification/${id}/read`)
}
