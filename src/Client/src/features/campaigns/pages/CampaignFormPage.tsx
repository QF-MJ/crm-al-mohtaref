import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { ArrowRight, Plus, UploadCloud } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { getApiErrorMessage } from '@/api/errors'
import { paths } from '@/app/router/paths'
import { useLookups } from '@/features/lookups/api/lookups.queries'
import { useCampaign, useCampaignQuestions, useImportCampaign, useUpdateCampaign, useUpdateQuestion } from '../api/campaigns.queries'
import { CampaignStatusBadge } from '../components/CampaignStatusBadge'
import type { CampaignQuestion, QuestionType, UpdateQuestionRequest } from '../types'

interface CampaignFormState {
  title: string
  description: string
  status: string
  trackId: string
  url: string
  budget: string
  platformIds: number[]
  questionIds: number[]
}

function errorText(error: unknown, fallback: string) {
  return getApiErrorMessage(error, fallback)
}

export function CampaignCreateUnavailablePage() {
  const { t } = useTranslation()
  return (
    <section className="mx-auto max-w-3xl space-y-4 rounded-xl border bg-white p-8 text-center" role="status">
      <h1 className="text-xl font-bold">{t('campaigns.newCampaign')}</h1>
      <p className="text-sm text-slate-600">{t('campaigns.createUnavailable')}</p>
      <Link to={paths.campaigns} className="inline-flex text-sm font-medium text-primary hover:underline">{t('campaigns.backToList')}</Link>
    </section>
  )
}

export function CampaignFormPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { campaignId: campaignIdParam } = useParams()
  const campaignId = campaignIdParam && /^\d+$/.test(campaignIdParam) ? Number(campaignIdParam) : undefined
  const campaignQuery = useCampaign(campaignId)
  const questionsQuery = useCampaignQuestions(campaignId)
  const lookupsQuery = useLookups()
  const updateCampaign = useUpdateCampaign(campaignId ?? 0)
  const importMutation = useImportCampaign(campaignId ?? 0)
  const [form, setForm] = useState<CampaignFormState | null>(null)
  const [editingQuestion, setEditingQuestion] = useState<number | null>(null)

  useEffect(() => {
    const campaign = campaignQuery.data
    if (!campaign) return
    setForm({
      title: campaign.title,
      description: campaign.description ?? '',
      status: campaign.status === undefined ? '' : String(campaign.status),
      trackId: campaign.trackId === undefined ? '' : String(campaign.trackId),
      url: campaign.url ?? '',
      budget: campaign.budget === undefined ? '' : String(campaign.budget),
      platformIds: campaign.platforms?.map((platform) => platform.id) ?? [],
      questionIds: campaign.questions?.map((question) => question.id) ?? [],
    })
  }, [campaignQuery.data])

  const sheetQuestions = questionsQuery.data?.questions
  useEffect(() => {
    if (!sheetQuestions) return
    setForm((current) => current
      ? { ...current, questionIds: sheetQuestions.filter((question) => question.isSelected).map((question) => question.id) }
      : current)
  }, [sheetQuestions])

  const campaign = campaignQuery.data
  const selectedQuestions = questionsQuery.data?.questions ?? campaign?.questions ?? []
  const activeStatusName = lookupsQuery.data?.campaignStatuses?.find((item) => item.id === campaign?.status)?.name
  const currentStatusName = lookupsQuery.data?.campaignStatuses?.find((item) => item.id === Number(form?.status))?.name
  const questionIds = useMemo(() => new Set(form?.questionIds ?? []), [form?.questionIds])

  if (campaignId === undefined) return <CampaignCreateUnavailablePage />
  if (campaignQuery.isLoading || lookupsQuery.isLoading) {
    return <p className="p-8 text-center text-sm text-slate-500" role="status">{t('campaigns.loading')}</p>
  }
  if (campaignQuery.isError || !campaign || lookupsQuery.isError || !lookupsQuery.data) {
    const error = campaignQuery.error ?? lookupsQuery.error
    return (
      <section className="mx-auto max-w-3xl space-y-4 rounded-xl border bg-white p-8 text-center" role="alert">
        <p className="text-sm text-destructive">{errorText(error, t('campaigns.error'))}</p>
        <Button type="button" variant="outline" onClick={() => { void campaignQuery.refetch(); void lookupsQuery.refetch() }}>{t('campaigns.retry')}</Button>
      </section>
    )
  }
  if (!form) return null

  const submit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const status = Number(form.status)
    const trackId = Number(form.trackId)
    const budget = form.budget === '' ? undefined : Number(form.budget)
    if (
      !form.title.trim() ||
      !form.status ||
      !form.trackId ||
      !Number.isInteger(status) ||
      !Number.isInteger(trackId)
    ) {
      toast.error(t('campaigns.requiredFields'))
      return
    }
    if (budget !== undefined && (!Number.isFinite(budget) || budget < 0)) {
      toast.error(t('campaigns.invalidBudget'))
      return
    }

    updateCampaign.mutate({
      title: form.title.trim(),
      description: form.description,
      status,
      trackId,
      url: form.url,
      budget,
      platformIds: form.platformIds,
      questionIds: form.questionIds,
    }, {
      onSuccess: () => toast.success(t('campaigns.saved')),
      onError: (error) => toast.error(errorText(error, t('campaigns.error'))),
    })
  }

  const toggleId = (id: number, selected: boolean, key: 'platformIds' | 'questionIds') => {
    setForm((current) => {
      if (!current) return current
      const currentIds = current[key]
      const nextIds = selected
        ? [...currentIds, id]
        : currentIds.filter((value) => value !== id)
      return key === 'platformIds'
        ? { ...current, platformIds: nextIds }
        : { ...current, questionIds: nextIds }
    })
  }

  const importFromSheet = () => {
    if (!form.url.trim()) {
      toast.error(t('campaigns.sheetRequired'))
      return
    }
    importMutation.mutate(undefined, {
      onSuccess: (result) => {
        toast.success(t('campaigns.importResult', { importedCount: result.importedCount, skippedCount: result.skippedCount }))
        void questionsQuery.refetch()
      },
      onError: (error) => toast.error(errorText(error, t('campaigns.error'))),
    })
  }

  return (
    <section className="-m-6 min-h-full space-y-5 bg-slate-50 p-5 sm:p-6" dir="rtl">
      <header>
        <p className="mb-1 text-xs text-slate-400"><Link to={paths.campaigns} className="hover:text-primary">{t('campaigns.title')}</Link> / {t('campaigns.review')}</p>
        <h1 className="text-2xl font-bold text-slate-800">{campaign.title}</h1>
      </header>

      <section className="flex flex-wrap items-center justify-between gap-4 rounded-xl border bg-white p-4 sm:px-6">
        <div>
          <p className="text-xs text-slate-400">{t('campaigns.campaignNumber', { id: campaign.id })}</p>
          <p className="mt-1 text-xs text-slate-500">{lookupsQuery.data.tracks?.find((track) => track.id === campaign.trackId)?.name ?? '—'}</p>
        </div>
        <CampaignStatusBadge label={activeStatusName} status={campaign.status} />
      </section>

      <form onSubmit={submit} className="grid gap-5 xl:grid-cols-[1.25fr_0.75fr]">
        <div className="space-y-5">
          <section className="space-y-4 rounded-xl border bg-white p-5 sm:p-6">
            <h2 className="border-b pb-3 font-semibold text-slate-800">{t('campaigns.editDetails')}</h2>
            <div className="space-y-2">
              <Label htmlFor="campaignTitle">{t('campaigns.titleField')}</Label>
              <Input
                id="campaignTitle"
                required
                value={form.title}
                onChange={(event) =>
                  setForm((current) =>
                    current ? { ...current, title: event.target.value } : current,
                  )
                }
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="campaignDescription">{t('campaigns.description')}</Label>
              <textarea
                id="campaignDescription"
                value={form.description}
                onChange={(event) =>
                  setForm((current) =>
                    current ? { ...current, description: event.target.value } : current,
                  )
                }
                rows={3}
                className="w-full rounded-md border border-input bg-white p-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
              />
            </div>
            <div className="grid gap-4 sm:grid-cols-2">
              <LookupSelect label={t('campaigns.track')} value={form.trackId} onChange={(trackId) => setForm((current) => current ? { ...current, trackId } : current)} options={lookupsQuery.data.tracks ?? []} emptyLabel={t('campaigns.allTracks')} fallback={campaign.trackId !== undefined && !lookupsQuery.data.tracks?.some((track) => track.id === campaign.trackId) ? { id: campaign.trackId, name: `#${campaign.trackId}` } : undefined} />
              <LookupSelect
                label={t('campaigns.status')}
                value={form.status}
                onChange={(status) =>
                  setForm((current) => (current ? { ...current, status } : current))
                }
                options={lookupsQuery.data.campaignStatuses ?? []}
                emptyLabel={t('campaigns.allStatuses')}
                fallback={
                  campaign.status !== undefined && !currentStatusName
                    ? { id: campaign.status, name: `#${campaign.status}` }
                    : undefined
                }
              />
            </div>
            <div className="grid gap-4 sm:grid-cols-2">
              <label className="space-y-2 text-sm font-medium">
                <span>{t('campaigns.sheetUrl')}</span>
                <Input
                  type="url"
                  dir="ltr"
                  value={form.url}
                  onChange={(event) =>
                    setForm((current) =>
                      current ? { ...current, url: event.target.value } : current,
                    )
                  }
                />
              </label>
              <label className="space-y-2 text-sm font-medium">
                <span>{t('campaigns.budget')}</span>
                <Input
                  type="number"
                  min="0"
                  step="0.01"
                  value={form.budget}
                  onChange={(event) =>
                    setForm((current) =>
                      current ? { ...current, budget: event.target.value } : current,
                    )
                  }
                />
              </label>
            </div>
            <div className="flex flex-wrap gap-2 border-t pt-4">
              <Button type="submit" disabled={updateCampaign.isPending || !form.trackId || !form.status}>{updateCampaign.isPending ? t('campaigns.saving') : t('campaigns.save')}</Button>
              <Button type="button" variant="outline" onClick={() => navigate(paths.campaigns)}>{t('actions.cancel')}</Button>
              <Button type="button" variant="outline" onClick={importFromSheet} disabled={importMutation.isPending || !form.url.trim()}><UploadCloud aria-hidden="true" />{importMutation.isPending ? t('campaigns.importing') : t('campaigns.importCustomers')}</Button>
            </div>
            {importMutation.data?.errors?.length ? (
              <ul className="space-y-1 rounded-md border border-amber-200 bg-amber-50 p-3 text-sm text-amber-900" role="status">
                {importMutation.data.errors.map((item, index) => <li key={`${item.row ?? index}-${index}`}>{item.row ? `${t('campaigns.row')} ${item.row}: ` : ''}{item.message ?? ''}</li>)}
              </ul>
            ) : null}
          </section>
        </div>

        <div className="space-y-5">
          <section className="space-y-4 rounded-xl border bg-white p-5">
            <div className="border-b pb-3"><h2 className="font-semibold text-slate-800">{t('campaigns.platforms')}</h2><p className="mt-1 text-xs text-slate-500">{t('campaigns.platformsDescription')}</p></div>
            {(lookupsQuery.data.platforms ?? []).length ? lookupsQuery.data.platforms?.map((platform) => (
              <label key={platform.id} className="flex items-center justify-between gap-3 text-sm">
                <span>{platform.name}</span>
                <input type="checkbox" checked={form.platformIds.includes(platform.id)} onChange={(event) => toggleId(platform.id, event.target.checked, 'platformIds')} className="size-4 accent-primary" />
              </label>
            )) : (
              <div className="space-y-2 text-sm text-slate-500">
                {campaign.platforms?.map((platform) => <p key={platform.id}>{platform.name}</p>)}
                <p>{t('campaigns.lookupUnavailable')}</p>
              </div>
            )}
          </section>

          <section className="space-y-4 rounded-xl border bg-white p-5">
            <div className="flex items-start justify-between gap-3 border-b pb-3">
              <div><h2 className="font-semibold text-slate-800">{t('campaigns.questions')}</h2><p className="mt-1 text-xs text-slate-500">{t('campaigns.questionsDescription')}</p></div>
              <Button type="button" variant="outline" size="sm" onClick={() => void questionsQuery.refetch()} disabled={questionsQuery.isFetching}><Plus aria-hidden="true" />{t('campaigns.readSheet')}</Button>
            </div>
            {questionsQuery.isError && <p className="text-sm text-destructive" role="alert">{errorText(questionsQuery.error, t('campaigns.error'))}</p>}
            {questionsQuery.isLoading ? <p className="text-sm text-slate-500" role="status">{t('campaigns.loading')}</p> : selectedQuestions.length ? selectedQuestions.map((question) => (
              <div key={question.id} className="space-y-3 rounded-md border p-3">
                <label className="flex items-start gap-3 text-sm leading-6">
                  <input type="checkbox" checked={questionIds.has(question.id)} onChange={(event) => toggleId(question.id, event.target.checked, 'questionIds')} className="mt-1 size-4 accent-primary" />
                  <span>{question.question}</span>
                </label>
                <div className="flex flex-wrap items-center justify-between gap-2 text-xs text-slate-500">
                  <span>{question.type ? t(`campaigns.questionTypes.${question.type}`) : t('campaigns.questionUnconfigured')}</span>
                  <Button type="button" variant="outline" size="sm" onClick={() => setEditingQuestion(editingQuestion === question.id ? null : question.id)}>{editingQuestion === question.id ? t('actions.cancel') : t('campaigns.configureQuestion')}</Button>
                </div>
                {editingQuestion === question.id && <QuestionConfigurator question={question} campaignId={campaign.id} fields={lookupsQuery.data.fields ?? []} tags={lookupsQuery.data.tags ?? []} types={lookupsQuery.data.questionTypes ?? []} />}
              </div>
            )) : !questionsQuery.isError ? <p className="text-sm text-slate-500">{t('campaigns.noQuestions')}</p> : null}
            <p className="rounded-md bg-slate-50 p-3 text-xs leading-5 text-slate-500">{t('campaigns.questionsHint')}</p>
          </section>
        </div>
      </form>

      <footer className="rounded-lg border bg-white px-4 py-3 text-xs leading-6 text-slate-500">
        <Link to={paths.campaigns} className="inline-flex items-center gap-2 text-primary hover:underline"><ArrowRight aria-hidden="true" />{t('campaigns.backToList')}</Link>
      </footer>
    </section>
  )
}

function QuestionConfigurator({ question, campaignId, fields, tags, types }: { question: CampaignQuestion; campaignId: number; fields: Array<{ id: number; name: string }>; tags: Array<{ id: number; tag: string }>; types: Array<{ id: number; name: string }> }) {
  const { t } = useTranslation()
  const updateMutation = useUpdateQuestion(campaignId)
  const [text, setText] = useState(question.question)
  const [type, setType] = useState<QuestionType | ''>(question.type ?? '')
  const [fieldId, setFieldId] = useState(question.fieldId ? String(question.fieldId) : '')
  const [answers, setAnswers] = useState((question.answers ?? []).map((item) => ({ tagId: String(item.tagId), answer: item.answer })))

  useEffect(() => {
    setText(question.question)
    setType(question.type ?? '')
    setFieldId(question.fieldId ? String(question.fieldId) : '')
    const savedAnswers = (question.answers ?? []).map((item) => ({
      tagId: String(item.tagId),
      answer: item.answer,
    }))
    const minimumAnswers = question.type === 2 ? 1 : question.type === 3 ? 2 : 0
    setAnswers([
      ...savedAnswers,
      ...Array.from(
        { length: Math.max(0, minimumAnswers - savedAnswers.length) },
        () => ({ tagId: '', answer: '' }),
      ),
    ])
  }, [question])

  const updateAnswer = (index: number, key: 'tagId' | 'answer', value: string) => {
    setAnswers((current) => current.map((answer, answerIndex) => answerIndex === index ? { ...answer, [key]: value } : answer))
  }

  const changeType = (nextType: QuestionType | '') => {
    setType(nextType)
    if (nextType === 2) {
      setAnswers((current) => current.length ? current.slice(0, 1) : [{ tagId: '', answer: '' }])
    } else if (nextType === 3) {
      setAnswers((current) => [
        ...current,
        ...Array.from({ length: Math.max(0, 2 - current.length) }, () => ({ tagId: '', answer: '' })),
      ])
    }
  }

  const save = () => {
    if (!type || !text.trim()) {
      toast.error(t('campaigns.questionRequired'))
      return
    }
    let body: UpdateQuestionRequest
    if (type === 1) {
      if (!fieldId) {
        toast.error(t('campaigns.fieldRequired'))
        return
      }
      body = { question: text.trim(), type, fieldId: Number(fieldId), answers: [] }
    } else {
      const requiredCount = type === 2 ? 1 : 2
      const validAnswers = (type === 2 ? answers.slice(0, 1) : answers)
        .filter((answer) => answer.tagId && answer.answer.trim())
        .map((answer) => ({ tagId: Number(answer.tagId), answer: answer.answer.trim() }))
      if (validAnswers.length < requiredCount || (type === 2 && validAnswers.length !== 1)) {
        toast.error(t(type === 2 ? 'campaigns.booleanAnswerRequired' : 'campaigns.choiceAnswersRequired'))
        return
      }
      body = { question: text.trim(), type, fieldId: null, answers: validAnswers }
    }
    updateMutation.mutate({ id: question.id, body }, {
      onSuccess: () => toast.success(t('campaigns.questionSaved')),
      onError: (error) => toast.error(errorText(error, t('campaigns.error'))),
    })
  }

  return (
    <div className="space-y-3 border-t pt-3">
      <label className="block space-y-1 text-xs text-slate-500"><span>{t('campaigns.questionText')}</span><Input value={text} onChange={(event) => setText(event.target.value)} /></label>
      <label className="block space-y-1 text-xs text-slate-500">
        <span>{t('campaigns.questionType')}</span>
        <select className="h-9 w-full rounded-md border bg-white px-3 text-sm" value={type} onChange={(event) => changeType(parseQuestionType(event.target.value))}>
          <option value="">{t('campaigns.selectQuestionType')}</option>
          {[1, 2, 3].map((id) => <option key={id} value={id}>{types.find((item) => item.id === id)?.name ?? t(`campaigns.questionTypes.${id}`)}</option>)}
        </select>
      </label>
      {type === 1 ? (
        <label className="block space-y-1 text-xs text-slate-500"><span>{t('campaigns.field')}</span><select className="h-9 w-full rounded-md border bg-white px-3 text-sm" value={fieldId} onChange={(event) => setFieldId(event.target.value)}><option value="">{t('campaigns.selectField')}</option>{fields.map((field) => <option key={field.id} value={field.id}>{field.name}</option>)}</select></label>
      ) : type ? (
        <div className="space-y-2">
          {(type === 2 ? answers.slice(0, 1) : answers).map((answer, index) => (
            <div key={index} className="grid gap-2 sm:grid-cols-[1fr_1fr_auto]">
              <Input value={answer.answer} placeholder={t('campaigns.answer')} onChange={(event) => updateAnswer(index, 'answer', event.target.value)} />
              <select className="h-9 rounded-md border bg-white px-3 text-sm" value={answer.tagId} onChange={(event) => updateAnswer(index, 'tagId', event.target.value)}><option value="">{t('campaigns.selectTag')}</option>{tags.map((tag) => <option key={tag.id} value={tag.id}>{tag.tag}</option>)}</select>
              {type === 3 && <Button type="button" variant="outline" size="sm" onClick={() => setAnswers((current) => current.filter((_, answerIndex) => answerIndex !== index))}>{t('users.removePermission')}</Button>}
            </div>
          ))}
          {type === 3 && <Button type="button" variant="outline" size="sm" onClick={() => setAnswers((current) => [...current, { tagId: '', answer: '' }])}>{t('campaigns.addAnswer')}</Button>}
        </div>
      ) : null}
      <Button type="button" size="sm" onClick={save} disabled={updateMutation.isPending}>{updateMutation.isPending ? t('campaigns.saving') : t('campaigns.saveQuestion')}</Button>
      {type === 1 && fields.length === 0 && <p className="text-xs text-destructive">{t('campaigns.lookupUnavailable')}</p>}
      {type !== 1 && type && tags.length === 0 && <p className="text-xs text-destructive">{t('campaigns.lookupUnavailable')}</p>}
    </div>
  )
}

function parseQuestionType(value: string): QuestionType | '' {
  if (value === '1') return 1
  if (value === '2') return 2
  if (value === '3') return 3
  return ''
}

function LookupSelect({ label, value, onChange, options, emptyLabel, fallback }: { label: string; value: string; onChange: (value: string) => void; options: Array<{ id: number; name: string }>; emptyLabel: string; fallback?: { id: number; name: string } }) {
  const values = fallback && !options.some((item) => item.id === fallback.id) ? [...options, fallback] : options
  return <label className="space-y-2 text-sm font-medium"><span>{label}</span><select className="h-9 w-full rounded-md border border-input bg-white px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50" value={value} onChange={(event) => onChange(event.target.value)}><option value="">{emptyLabel}</option>{values.map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}</select></label>
}
