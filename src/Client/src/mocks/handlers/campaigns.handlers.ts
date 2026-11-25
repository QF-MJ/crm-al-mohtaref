import { http, HttpResponse, delay } from 'msw'
import { env } from '@/lib/env'
import type { CampaignListItem, CampaignsPage } from '@/features/campaigns'

// محاكاة لعقد GET /api/campaigns في الـ yaml حتى ينفّذه الباك
// (GetCampaignsPageQueryHandler ما زال صنفاً فارغاً).

const guid = (suffix: number) => `00000000-0000-0000-0000-${String(suffix).padStart(12, '0')}`
const PAGE_SIZE = 20

// نفس أسماء SeedRules.Lookups["CampaginStatus"] في الباك
export const campaignStatuses = [
  { id: guid(601), name: 'مسودة' },
  { id: guid(602), name: 'نشطة' },
  { id: guid(603), name: 'متوقفة مؤقتاً' },
  { id: guid(604), name: 'منتهية' },
  { id: guid(605), name: 'ملغاة' },
]

// الباك لا يملك قائمة منصات ثابتة بعد؛ هذه من التصميم
export const platforms = [
  { id: guid(701), name: 'Facebook' },
  { id: guid(702), name: 'Instagram' },
]

// نفس معرّفات المسارات في users.handlers (البرمجة للأطفال، اللغة الإنجليزية للأطفال)
const TRACK_IDS = [guid(301), guid(302)]

interface MockCampaign extends CampaignListItem {
  platformIds: string[]
}

const titles = [
  'البرمجة للصغار',
  'تهيئة المسار الدراسي',
  'لقاء التعريف بالبرامج',
  'أساسيات الروبوت',
  'مهارات التفكير المنطقي',
  'الإنجليزية من الصفر',
  'محادثة إنجليزية للأطفال',
  'مخيم البرمجة الصيفي',
  'تحدي سكراتش',
  'عرض العودة للمدارس',
  'برمجة الألعاب',
  'قصص بالإنجليزية',
]

// عدد كافٍ لتجربة الانتقال للصفحة الثانية (PAGE_SIZE = 20)
const campaigns: MockCampaign[] = Array.from({ length: 26 }, (_, index) => ({
  id: guid(900 + index),
  title: `${titles[index % titles.length]}${index >= titles.length ? ` ${Math.floor(index / titles.length) + 1}` : ''}`,
  statusId: campaignStatuses[index % campaignStatuses.length].id,
  trackId: TRACK_IDS[index % TRACK_IDS.length],
  platformIds: index % 3 === 0 ? [platforms[0].id] : platforms.map((platform) => platform.id),
}))

export const campaignsHandlers = [
  http.get(`${env.apiUrl}/campaigns`, async ({ request }) => {
    await delay(200)
    const params = new URL(request.url).searchParams
    const pageNumber = Math.max(1, Number(params.get('pageNumber')) || 1)
    const title = params.get('title')?.trim().toLocaleLowerCase('ar')
    const status = params.get('status')
    const trackId = params.get('trackId')
    const platformId = params.get('platformId')

    // كل الفلاتر اختيارية وتُجمع بـ AND، والعنوان مطابقة جزئية (كما في العقد)
    const filtered = campaigns.filter(
      (campaign) =>
        (!title || campaign.title.toLocaleLowerCase('ar').includes(title)) &&
        (!status || campaign.statusId === status) &&
        (!trackId || campaign.trackId === trackId) &&
        (!platformId || campaign.platformIds.includes(platformId)),
    )

    const start = (pageNumber - 1) * PAGE_SIZE
    const body: CampaignsPage = {
      // الرد يحمل حقول CampaignListItemDto فقط (بدون platformIds الداخلية)
      items: filtered
        .slice(start, start + PAGE_SIZE)
        .map(({ id, title: campaignTitle, statusId, trackId: campaignTrackId }) => ({
          id,
          title: campaignTitle,
          statusId,
          trackId: campaignTrackId,
        })),
      page: { pageNumber, hasNextPage: start + PAGE_SIZE < filtered.length },
    }
    return HttpResponse.json(body)
  }),
]
