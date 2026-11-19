export interface ApiError {
  status: number
  message: string
  fieldErrors?: Record<string, string[]>
}

/** صفحة واحدة من ردود الباك: لا يوجد إجمالي، فقط رقم الصفحة وهل توجد تالية */
export interface PageInfo {
  pageNumber: number
  hasNextPage: boolean
}
