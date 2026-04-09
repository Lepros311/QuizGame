import { publicApi } from './client'
import type { AuthResultDto } from '../types/auth'

export async function loginRequest(
  email: string,
  password: string,
): Promise<AuthResultDto> {
  const { data } = await publicApi.post<AuthResultDto>('/api/auth/login', {
    email,
    password,
  })
  return data
}

export async function registerRequest(
  username: string,
  email: string,
  password: string,
): Promise<AuthResultDto> {
  const { data } = await publicApi.post<AuthResultDto>('/api/auth/register', {
    username,
    email,
    password,
  })
  return data
}