import type { PageInfo } from '@/types/common'

// مطابق لعقد الـ yaml (docs/design/api/crm-api-v0.openapi.yaml): كل المعرّفات Guid.
// ملاحظة: CampaignListItemDto الحالي في فرع get-campaign بالباك لا يطابق العقد (ينقصه id)،
// ونحن نتبع العقد.

/** عنصر CampaignListItemDto كما يعيده GET /api/campaigns */
export interface CampaignListItem {
  id: string
  title: string
  statusId: string
  trackId: string
}

/** CampaignsPageDto */
export interface CampaignsPage {
  items: CampaignListItem[]
  page?: PageInfo
}

/** GetCampaignsRequest: كل الفلاتر اختيارية وتُجمع بـ AND */
export interface CampaignFilters {
  pageNumber: number
  /** مطابقة جزئية بدون حساسية لحالة الأحرف */
  title?: string
  /** Guid حالة الحملة (اسم الحقل في العقد: Status) */
  status?: string
  /** Guid من lookups: Tracks */
  trackId?: string
  /** Guid: الحملات التي تعمل على هذه المنصة */
  platformId?: string
}
