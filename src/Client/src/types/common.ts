// شكل الترقيم مؤقت لحين اعتماد شكل الباك (قرار 2 بقائمة القرارات المفتوحة)
export interface Paginated<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}

export interface ApiError {
  status: number
  message: string
  fieldErrors?: Record<string, string[]>
}
