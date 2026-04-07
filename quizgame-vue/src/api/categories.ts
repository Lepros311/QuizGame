import type { CategoryDto } from '../types/api'
import { apiClient } from './client'

export async function fetchCategories(): Promise<CategoryDto[]> {
  const { data } = await apiClient.get<CategoryDto[]>('/api/category')
  return data
}