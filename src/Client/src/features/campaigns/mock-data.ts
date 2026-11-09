import type { Campaign } from './types'

const sampleQuestions = [
  { id: 'q1', text: 'ما البريد الإلكتروني للتواصل؟', selected: true },
  { id: 'q2', text: 'ما هو المجال الدراسي الذي تهتم به؟', selected: true },
  { id: 'q3', text: 'ما البرنامج الذي ترغب بالتسجيل فيه؟', selected: false },
]

const campaigns: Campaign[] = [
  {
    id: 13,
    title: 'البرمجة للصغار',
    description: 'تعريف الأهالي بمسار البرمجة للصغار والبرامج المناسبة للناشئين.',
    trackId: 1,
    trackName: 'البرمجة',
    status: 'draft',
    platforms: ['Facebook', 'Instagram'],
    budget: 250,
    sheetUrl: '',
    startDate: '2026-11-10',
    endDate: '2026-12-10',
    importedCustomers: 0,
    questions: sampleQuestions.map((question) => ({ ...question })),
  },
  {
    id: 15,
    title: 'تهيئة المسار الدراسي',
    description: 'حملة تعريفية بالمسارات التعليمية المتاحة.',
    trackId: 2,
    trackName: 'عام',
    status: 'active',
    platforms: ['Instagram'],
    budget: 500,
    sheetUrl: '',
    startDate: '2026-10-01',
    endDate: '2026-12-01',
    importedCustomers: 24,
    questions: sampleQuestions.map((question) => ({ ...question })),
  },
  {
    id: 18,
    title: 'لقاء التعريف بالبرامج',
    description: 'التسجيل في اللقاء التعريفي بالبرامج الأكاديمية.',
    trackId: 3,
    trackName: 'التوجيه الأكاديمي',
    status: 'paused',
    platforms: ['Facebook'],
    budget: 350,
    sheetUrl: '',
    startDate: '2026-09-01',
    endDate: '2026-10-01',
    importedCustomers: 18,
    questions: sampleQuestions.map((question) => ({ ...question })),
  },
  {
    id: 21,
    title: 'أساسيات الروبوت',
    description: 'تعريف الطلاب بأساسيات الروبوت والبرمجة.',
    trackId: 1,
    trackName: 'البرمجة',
    status: 'active',
    platforms: ['Facebook', 'Instagram'],
    budget: 700,
    sheetUrl: '',
    startDate: '2026-11-01',
    endDate: '2026-12-31',
    importedCustomers: 21,
    questions: sampleQuestions.map((question) => ({ ...question })),
  },
  {
    id: 24,
    title: 'مهارات التفكير المنطقي',
    description: 'حملة توعوية حول مهارات التفكير المنطقي.',
    trackId: 2,
    trackName: 'عام',
    status: 'completed',
    platforms: ['Instagram'],
    budget: 400,
    sheetUrl: '',
    startDate: '2026-07-01',
    endDate: '2026-08-01',
    importedCustomers: 32,
    questions: sampleQuestions.map((question) => ({ ...question })),
  },
  {
    id: 26,
    title: 'رحلة إلى عالم التصميم',
    description: 'استكشاف مبادئ التصميم الإبداعي.',
    trackId: 4,
    trackName: 'التصميم',
    status: 'draft',
    platforms: ['Facebook'],
    budget: null,
    sheetUrl: '',
    startDate: '',
    endDate: '',
    importedCustomers: 0,
    questions: sampleQuestions.map((question) => ({ ...question })),
  },
]

export const campaignTracks = [
  { id: 1, name: 'البرمجة' },
  { id: 2, name: 'عام' },
  { id: 3, name: 'التوجيه الأكاديمي' },
  { id: 4, name: 'التصميم' },
]

export const campaignPlatforms = ['Facebook', 'Instagram', 'TikTok', 'Google']

export function getMockCampaigns() {
  return campaigns
}

export function getMockCampaign(id: number) {
  return campaigns.find((campaign) => campaign.id === id)
}

export function saveMockCampaign(input: Omit<Campaign, 'id'> & { id?: number }) {
  if (input.id === undefined) {
    const campaign = { ...input, id: Math.max(0, ...campaigns.map((item) => item.id)) + 1 }
    campaigns.unshift(campaign)
    return campaign
  }

  const index = campaigns.findIndex((campaign) => campaign.id === input.id)
  const campaign = { ...input, id: input.id }
  if (index === -1) campaigns.unshift(campaign)
  else campaigns[index] = campaign
  return campaign
}
