import { useEffect } from 'react'
import { Controller, useFieldArray, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { paths } from '@/app/router/paths'
import type { LookupItem } from '../types'
import { createUserSchema, userSchema, type UserFormValues } from '../schemas/user.schema'

interface UserFormProps {
  defaultValues: UserFormValues
  departments: LookupItem[]
  roles: LookupItem[]
  isPending: boolean
  createMode?: boolean
  showStatus?: boolean
  showPassword?: boolean
  submitLabel?: string
  accountFieldsClassName?: string
  onSubmit: (values: UserFormValues) => void
}

export function UserForm({
  defaultValues,
  departments,
  roles,
  isPending,
  createMode = false,
  showStatus = true,
  showPassword = false,
  submitLabel,
  accountFieldsClassName = 'sm:grid-cols-2',
  onSubmit,
}: UserFormProps) {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const {
    register,
    handleSubmit,
    reset,
    control,
    formState: { errors },
  } = useForm<UserFormValues>({
    resolver: zodResolver(createMode ? createUserSchema : userSchema),
    defaultValues: { ...defaultValues, password: defaultValues.password ?? '' },
  })
  const { fields, append, remove } = useFieldArray({ control, name: 'permissions' })

  useEffect(() => {
    reset({ ...defaultValues, password: defaultValues.password ?? '' })
  }, [defaultValues, reset])

  return (
    <form
      onSubmit={handleSubmit(onSubmit)}
      noValidate
      className="space-y-5 rounded-xl border bg-white p-5 sm:p-7"
    >
      <section className="space-y-4">
        <div>
          <h2 className="font-semibold text-slate-800">{t('users.accountDetails')}</h2>
          <p className="mt-1 text-xs text-slate-500">{t('users.accountDetailsDescription')}</p>
        </div>
        <div className={`grid gap-5 ${accountFieldsClassName}`}>
          <div className="space-y-2">
            <Label htmlFor="fullName">{t('users.name')}</Label>
            <Input
              id="fullName"
              dir="auto"
              maxLength={createMode ? undefined : 200}
              {...register('fullName')}
            />
            {errors.fullName && <p className="text-sm text-destructive">{t('users.required')}</p>}
          </div>
          <div className="space-y-2">
            <Label htmlFor="accountName">{t('users.username')}</Label>
            <Input id="accountName" dir="auto" {...register('accountName')} />
            {errors.accountName && (
              <p className="text-sm text-destructive">{t('users.required')}</p>
            )}
          </div>
          <div className="space-y-2">
            <Label htmlFor="email">{t('users.email')}</Label>
            <Input
              id="email"
              type={createMode ? 'text' : 'email'}
              dir="ltr"
              {...register('email')}
            />
            {errors.email && <p className="text-sm text-destructive">{t('users.invalidEmail')}</p>}
          </div>
          {showPassword && (
            <div className="space-y-2">
              <Label htmlFor="password">{t('users.password')}</Label>
              <Input
                id="password"
                type="password"
                autoComplete="new-password"
                {...register('password')}
              />
              {errors.password && <p className="text-sm text-destructive">{t('users.required')}</p>}
            </div>
          )}
          {showStatus && (
            <div className="space-y-2">
              <Label htmlFor="isActive">{t('users.status')}</Label>
              <Controller
                name="isActive"
                control={control}
                render={({ field }) => (
                  <select
                    id="isActive"
                    name={field.name}
                    ref={field.ref}
                    onBlur={field.onBlur}
                    value={String(field.value)}
                    onChange={(event) => field.onChange(event.target.value === 'true')}
                    className="h-9 w-full rounded-md border border-input bg-white px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
                  >
                    <option value="true">{t('users.active')}</option>
                    <option value="false">{t('users.inactive')}</option>
                  </select>
                )}
              />
            </div>
          )}
        </div>
      </section>

      <section className="space-y-3 border-t pt-5">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <h2 className="font-semibold text-slate-800">{t('users.permissionsTitle')}</h2>
            <p className="mt-1 text-xs text-slate-500">{t('users.permissionsDescription')}</p>
          </div>
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={() =>
              append({ roleId: roles[0]?.id ?? 0, departmentId: departments[0]?.id ?? 0 })
            }
            disabled={!roles.length || !departments.length}
          >
            {t('users.addPermission')}
          </Button>
        </div>
        {fields.length === 0 && (
          <p className="text-sm text-slate-500">{t('users.noPermissions')}</p>
        )}
        {fields.map((field, index) => (
          <div key={field.id} className="grid gap-3 sm:grid-cols-[1fr_1fr_auto]">
            <label className="space-y-1.5 text-xs font-medium text-slate-500">
              <span>{t('users.role')}</span>
              <select
                className="h-9 w-full rounded-md border border-input bg-white px-3 text-sm text-slate-700 outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
                {...register(`permissions.${index}.roleId`, { valueAsNumber: true })}
              >
                {roles.map((role) => (
                  <option key={role.id} value={role.id}>
                    {role.name}
                  </option>
                ))}
              </select>
            </label>
            <label className="space-y-1.5 text-xs font-medium text-slate-500">
              <span>{t('users.department')}</span>
              <select
                className="h-9 w-full rounded-md border border-input bg-white px-3 text-sm text-slate-700 outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
                {...register(`permissions.${index}.departmentId`, { valueAsNumber: true })}
              >
                {departments.map((department) => (
                  <option key={department.id} value={department.id}>
                    {department.name}
                  </option>
                ))}
              </select>
            </label>
            <Button
              type="button"
              variant="outline"
              className="self-end"
              onClick={() => remove(index)}
            >
              {t('users.removePermission')}
            </Button>
          </div>
        ))}
        {errors.permissions && (
          <p className="text-sm text-destructive">{t('users.invalidPermission')}</p>
        )}
      </section>

      <div className="flex flex-wrap gap-3 border-t pt-5">
        <Button
          type="submit"
          disabled={isPending}
          className="bg-teal-700 text-white hover:bg-teal-800"
        >
          {isPending ? t('users.saving') : (submitLabel ?? t('users.saveChanges'))}
        </Button>
        <Button type="button" variant="outline" onClick={() => navigate(paths.users)}>
          {t('actions.cancel')}
        </Button>
      </div>
    </form>
  )
}
