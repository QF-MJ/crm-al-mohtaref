import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { ArrowRight, Plus, UploadCloud } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { paths } from '@/app/router/paths'
import { CampaignStatusBadge } from '../components/CampaignStatusBadge'
import { campaignPlatforms, campaignTracks, getMockCampaign, saveMockCampaign } from '../mock-data'
import type { Campaign, CampaignQuestion, CampaignStatus } from '../types'

const emptyCampaign: Omit<Campaign, 'id'> = {
  title: '',
  description: '',
  trackId: campaignTracks[0].id,
  trackName: campaignTracks[0].name,
  status: 'draft',
  platforms: [],
  budget: null,
  sheetUrl: '',
  startDate: '',
  endDate: '',
  importedCustomers: 0,
  questions: [
    { id: 'q1', text: 'ما البريد الإلكتروني للتواصل؟', selected: true },
    { id: 'q2', text: 'ما هو المجال الدراسي الذي تهتم به؟', selected: true },
    { id: 'q3', text: 'ما البرنامج الذي ترغب بالتسجيل فيه؟', selected: false },
  ],
}

export function CampaignFormPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { campaignId: campaignIdParam } = useParams()
  const campaignId =
    campaignIdParam && /^\d+$/.test(campaignIdParam) ? Number(campaignIdParam) : undefined
  const isNew = campaignIdParam === undefined
  const existingCampaign = useMemo(
    () => (campaignId === undefined ? undefined : getMockCampaign(campaignId)),
    [campaignId],
  )
  const [form, setForm] = useState<Omit<Campaign, 'id'>>(emptyCampaign)

  useEffect(() => {
    if (existingCampaign) {
      setForm(toFormValues(existingCampaign))
    } else if (isNew) {
      setForm({ ...emptyCampaign, questions: emptyCampaign.questions.map((question) => ({ ...question })) })
    }
  }, [existingCampaign, isNew])

  const updateForm = <K extends keyof Omit<Campaign, 'id'>>(
    key: K,
    value: Omit<Campaign, 'id'>[K],
  ) => setForm((current) => ({ ...current, [key]: value }))

  const submit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!form.title.trim()) {
      toast.error(t('campaigns.titleRequired'))
      return
    }
    if (!form.trackId) {
      toast.error(t('campaigns.trackRequired'))
      return
    }

    const track = campaignTracks.find((item) => item.id === form.trackId)
    const savedCampaign = saveMockCampaign({
      ...form,
      title: form.title.trim(),
      description: form.description.trim(),
      trackName: track?.name ?? '',
      id: existingCampaign?.id,
    })
    toast.success(t('campaigns.saved'))
    navigate(paths.campaignEdit(savedCampaign.id))
  }

  const togglePlatform = (platform: string) => {
    updateForm(
      'platforms',
      form.platforms.includes(platform)
        ? form.platforms.filter((item) => item !== platform)
        : [...form.platforms, platform],
    )
  }

  const toggleQuestion = (questionId: string) => {
    updateForm(
      'questions',
      form.questions.map((question) =>
        question.id === questionId ? { ...question, selected: !question.selected } : question,
      ),
    )
  }

  const importFromSheet = () => {
    if (!form.sheetUrl.trim()) {
      toast.error(t('campaigns.sheetRequired'))
      return
    }
    if (isNew) {
      toast.info(t('campaigns.saveBeforeImport'))
      return
    }
    toast.info(t('campaigns.importNotConnected'))
  }

  const readQuestionsFromSheet = () => {
    if (!form.sheetUrl.trim()) {
      toast.error(t('campaigns.sheetRequired'))
      return
    }
    toast.info(t('campaigns.questionsNotConnected'))
  }

  if (!isNew && (!campaignId || !existingCampaign)) {
    return (
      <section className="mx-auto max-w-3xl space-y-4 rounded-xl border bg-white p-8 text-center" role="alert">
        <p className="text-sm text-destructive">{t('campaigns.notFound')}</p>
        <Link to={paths.campaigns} className="text-sm font-medium text-primary hover:underline">
          {t('campaigns.backToList')}
        </Link>
      </section>
    )
  }

  return (
    <section className="-m-6 min-h-full space-y-5 bg-slate-50 p-5 sm:p-6" dir="rtl">
      <header className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <p className="mb-1 text-xs text-slate-400">
            <Link to={paths.campaigns} className="hover:text-primary">{t('campaigns.title')}</Link>
            {' / '}
            {isNew ? t('campaigns.newCampaign') : t('campaigns.review')}
          </p>
          <h1 className="text-2xl font-bold text-slate-800">
            {isNew ? t('campaigns.newCampaign') : form.title}
          </h1>
        </div>
      </header>

      <section className="flex flex-wrap items-center justify-between gap-4 rounded-xl border bg-white p-4 sm:px-6">
        <div>
          <p className="text-xs text-slate-400">{t('campaigns.campaignNumber', { id: existingCampaign?.id ?? '—' })}</p>
          <h2 className="mt-1 font-semibold text-slate-800">{form.title || t('campaigns.untitled')}</h2>
          <p className="mt-1 text-xs text-slate-500">{form.trackName}</p>
        </div>
        <div className="flex flex-wrap items-center gap-3">
          {!isNew && (
            <span className="rounded-full bg-slate-100 px-3 py-1 text-xs text-slate-600">
              {t('campaigns.importedCount', { count: form.importedCustomers })}
            </span>
          )}
          <CampaignStatusBadge status={form.status} />
        </div>
      </section>

      <form onSubmit={submit} className="grid gap-5 xl:grid-cols-[1.25fr_0.75fr]">
        <div className="space-y-5">
          <section className="space-y-4 rounded-xl border bg-white p-5 sm:p-6">
            <div className="border-b pb-3">
              <h2 className="font-semibold text-slate-800">{t('campaigns.editDetails')}</h2>
              <p className="mt-1 text-xs text-slate-500">{t('campaigns.formDescription')}</p>
            </div>
            <div className="space-y-2">
              <Label htmlFor="campaignTitle">{t('campaigns.titleField')}</Label>
              <Input
                id="campaignTitle"
                required
                value={form.title}
                onChange={(event) => updateForm('title', event.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="campaignDescription">{t('campaigns.description')}</Label>
              <textarea
                id="campaignDescription"
                value={form.description}
                onChange={(event) => updateForm('description', event.target.value)}
                rows={3}
                className="w-full rounded-md border border-input bg-white p-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
              />
            </div>
            <div className="grid gap-4 sm:grid-cols-2">
              <label className="space-y-2 text-sm font-medium">
                <span>{t('campaigns.track')}</span>
                <select
                  required
                  value={form.trackId}
                  onChange={(event) => {
                    const id = Number(event.target.value)
                    updateForm('trackId', id)
                    updateForm('trackName', campaignTracks.find((item) => item.id === id)?.name ?? '')
                  }}
                  className="h-9 w-full rounded-md border border-input bg-white px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
                >
                  {campaignTracks.map((track) => (
                    <option key={track.id} value={track.id}>{track.name}</option>
                  ))}
                </select>
              </label>
              <label className="space-y-2 text-sm font-medium">
                <span>{t('campaigns.status')}</span>
                <select
                  value={form.status}
                  onChange={(event) => updateForm('status', event.target.value as CampaignStatus)}
                  className="h-9 w-full rounded-md border border-input bg-white px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
                >
                  {(['draft', 'active', 'paused', 'completed'] as CampaignStatus[]).map((status) => (
                    <option key={status} value={status}>{t(`campaigns.statuses.${status}`)}</option>
                  ))}
                </select>
              </label>
            </div>
            <fieldset className="space-y-3 rounded-lg border bg-slate-50 p-4">
              <legend className="px-1 text-sm font-medium">{t('campaigns.scheduleRequired')}</legend>
              <div className="grid gap-4 sm:grid-cols-2">
                <label className="space-y-2 text-xs text-slate-600">
                  <span>{t('campaigns.startDate')}</span>
                  <Input type="date" value={form.startDate} onChange={(event) => updateForm('startDate', event.target.value)} />
                </label>
                <label className="space-y-2 text-xs text-slate-600">
                  <span>{t('campaigns.endDate')}</span>
                  <Input type="date" value={form.endDate} onChange={(event) => updateForm('endDate', event.target.value)} />
                </label>
              </div>
            </fieldset>
            <div className="grid gap-4 sm:grid-cols-2">
              <label className="space-y-2 text-sm font-medium">
                <span>{t('campaigns.sheetUrl')}</span>
                <Input
                  type="url"
                  dir="ltr"
                  placeholder="https://docs.google.com/spreadsheets/..."
                  value={form.sheetUrl}
                  onChange={(event) => updateForm('sheetUrl', event.target.value)}
                />
              </label>
              <label className="space-y-2 text-sm font-medium">
                <span>{t('campaigns.budget')}</span>
                <Input
                  type="number"
                  min="0"
                  step="0.01"
                  value={form.budget ?? ''}
                  onChange={(event) => updateForm('budget', event.target.value === '' ? null : Number(event.target.value))}
                />
              </label>
            </div>
            <div className="flex flex-wrap gap-2 border-t pt-4">
              <Button type="submit">{t('campaigns.save')}</Button>
              <Button type="button" variant="outline" onClick={() => navigate(paths.campaigns)}>
                {t('actions.cancel')}
              </Button>
              <Button type="button" variant="outline" onClick={importFromSheet}>
                <UploadCloud aria-hidden="true" />
                {t('campaigns.importCustomers')}
              </Button>
            </div>
          </section>
        </div>

        <div className="space-y-5">
          <section className="space-y-4 rounded-xl border bg-white p-5">
            <div className="border-b pb-3">
              <h2 className="font-semibold text-slate-800">{t('campaigns.platforms')}</h2>
              <p className="mt-1 text-xs text-slate-500">{t('campaigns.platformsDescription')}</p>
            </div>
            <div className="space-y-3">
              {campaignPlatforms.map((platform) => (
                <label key={platform} className="flex cursor-pointer items-center justify-between gap-3 text-sm">
                  <span>{platform}</span>
                  <input
                    type="checkbox"
                    checked={form.platforms.includes(platform)}
                    onChange={() => togglePlatform(platform)}
                    className="size-4 accent-primary"
                  />
                </label>
              ))}
            </div>
          </section>

          <section className="space-y-4 rounded-xl border bg-white p-5">
            <div className="flex items-start justify-between gap-3 border-b pb-3">
              <div>
                <h2 className="font-semibold text-slate-800">{t('campaigns.questions')}</h2>
                <p className="mt-1 text-xs text-slate-500">{t('campaigns.questionsDescription')}</p>
              </div>
              <Button type="button" variant="outline" size="sm" onClick={readQuestionsFromSheet}>
                <Plus aria-hidden="true" />
                {t('campaigns.readSheet')}
              </Button>
            </div>
            {form.questions.map((question) => (
              <QuestionOption
                key={question.id}
                question={question}
                onChange={() => toggleQuestion(question.id)}
              />
            ))}
            <p className="rounded-md bg-slate-50 p-3 text-xs leading-5 text-slate-500">
              {t('campaigns.questionsHint')}
            </p>
          </section>
        </div>
      </form>

      <footer className="rounded-lg border bg-white px-4 py-3 text-xs leading-6 text-slate-500">
        <Link to={paths.campaigns} className="inline-flex items-center gap-2 text-primary hover:underline">
          <ArrowRight aria-hidden="true" />
          {t('campaigns.backToList')}
        </Link>
      </footer>
    </section>
  )
}

function QuestionOption({
  question,
  onChange,
}: {
  question: CampaignQuestion
  onChange: () => void
}) {
  return (
    <label className="flex cursor-pointer items-start gap-3 rounded-md border p-3 text-sm leading-6">
      <input
        type="checkbox"
        checked={question.selected}
        onChange={onChange}
        className="mt-1 size-4 shrink-0 accent-primary"
      />
      <span>{question.text}</span>
    </label>
  )
}

function toFormValues(campaign: Campaign): Omit<Campaign, 'id'> {
  return {
    title: campaign.title,
    description: campaign.description,
    trackId: campaign.trackId,
    trackName: campaign.trackName,
    status: campaign.status,
    platforms: [...campaign.platforms],
    budget: campaign.budget,
    sheetUrl: campaign.sheetUrl,
    startDate: campaign.startDate,
    endDate: campaign.endDate,
    importedCustomers: campaign.importedCustomers,
    questions: campaign.questions.map((question) => ({ ...question })),
  }
}
