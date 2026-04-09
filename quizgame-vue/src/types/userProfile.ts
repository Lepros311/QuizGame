/** Matches UserProfileResponse from the API (camelCase JSON) */
export type UserProfileDto = {
  userId: string
  username: string
  email: string
  memberSince: string
  followersCount: number
  followingCount: number
  skillScore: number
}
