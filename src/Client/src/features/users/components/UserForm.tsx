import { useEffect, type ReactNode } from 'react'
import { Controller, useFieldArray, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useTranslation } from 'react-i18next'
import { AlertCircle, Plus, Trash2 } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select } from '@/components/ui/select'
import { createUserSchema, userFormSchema, type UserFormValues } from '../schemas/user.schema'
import type { LookupItem } from '../types'

interface UserFormProps {
  defaultValues: UserFormValues
  departments: LookupItem[]
  roles: LookupItem[]
  isPending: boolean
  createMode?: boolean
  submitLabel: string
  errorMessage?: string
  onSubmit: (values: UserFormValues) => void
  onCancel: () => void
}

export function UserForm({
  defaultValues,
  departments,
  roles,
  isPending,
  createMode = false,
  submitLabel,
  errorMessage,
  onSubmit,
  onCancel,
}: UserFormProps) {
  const { t } = useTranslation()
  const {
    register,
    handleSubmit,
    reset,
    control,
    formState: { errors },
  } = useForm<UserFormValues>({
    resolver: zodResolver(createMode ? createUserSchema : userFormSchema),
    defaultValues,
    mode: 'onSubmit',
  })
  const { fields, append, remove } = useFieldArray({ control, name: 'permissions' })

  // أخطاء مصفوفة الصلاحيات: خطأ على المصفوفة نفسها + أخطاء الصفوف
  const permissionsRootMessage = (errors.permissions as unknown as { message?: string } | undefined)
    ?.message
  const permissionRows = (Array.isArray(errors.permissions) ? errors.permissions : []) as Array<
    { roleId?: { message?: string } } | undefined
  >

  useEffect(() => {
    reset(defaultValues)
  }, [defaultValues, reset])

  const addPermission = () =>
    append({
      roleId: roles[0] ? String(roles[0].id) : '',
      departmentId: departments[0] ? String(departments[0].id) : '',
    })

  return (
    <form
      onSubmit={handleSubmit(onSubmit)}
      noValidate
      className="space-y-5 rounded-2xl border border-slate-200 bg-white p-5 shadow-sm sm:p-7"
    >
      {errorMessage && (
        <div
          role="alert"
          className="flex items-start gap-3 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-800"
        >
          <AlertCircle aria-hidden="true" className="mt-0.5 size-[18px] shrink-0" />
          <p className="leading-6">{errorMessage}</p>
        </div>
      )}

      <section className="space-y-4">
        <div>
          <h2 className="font-semibold text-slate-900">{t('users.accountDetails')}</h2>
          <p className="mt-1 text-xs leading-6 text-slate-500">
            {t('users.accountDetailsDescription')}
          </p>
        </div>

        <div className="grid gap-5 sm:grid-cols-2">
          <Field label={t('users.fullName')} htmlFor="fullName" error={errors.fullName?.message}>
            <Input id="fullName" dir="auto" autoComplete="off" {...register('fullName')} />
          </Field>

          <Field
            label={t('users.username')}
            htmlFor="accountName"
            error={errors.accountName?.message}
            hint={t('users.usernameHint')}
          >
            <Input id="accountName" dir="ltr" autoComplete="off" {...register('accountName')} />
          </Field>

          <Field label={t('users.email')} htmlFor="email" error={errors.email?.message}>
            <Input id="email" type="email" dir="ltr" autoComplete="off" {...register('email')} />
          </Field>

          {createMode && (
            <Field
              label={t('users.password')}
              htmlFor="password"
              error={errors.password?.message}
              hint={t('users.passwordHint')}
            >
              <Input
                id="password"
                type="password"
                dir="ltr"
                autoComplete="new-password"
                {...register('password')}
              />
            </Field>
          )}

          {!createMode && (
            <Field label={t('users.status')} htmlFor="isActive" error={errors.isActive?.message}>
              <Controller
                name="isActive"
                control={control}
                render={({ field }) => (
                  <Select
                    id="isActive"
                    ref={field.ref}
                    onBlur={field.onBlur}
                    value={String(field.value)}
                    onValueChange={(value) => field.onChange(value === 'true')}
                    options={[
                      { value: 'true', label: t('users.active') },
                      { value: 'false', label: t('users.inactive') },
                    ]}
                  />
                )}
              />
            </Field>
          )}
        </div>
      </section>

      <section className="space-y-3 border-t border-slate-100 pt-5">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <h2 className="font-semibold text-slate-900">{t('users.permissionsTitle')}</h2>
            <p className="mt-1 text-xs leading-6 text-slate-500">
              {t('users.permissionsDescription')}
            </p>
          </div>
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={addPermission}
            disabled={!roles.length || !departments.length || isPending}
          >
            <Plus aria-hidden="true" />
            {t('users.addPermission')}
          </Button>
        </div>

        {fields.length === 0 ? (
          <p className="rounded-xl bg-amber-50 px-4 py-3 text-sm text-amber-800">
            {t('users.noPermissions')}
          </p>
        ) : (
          <ul className="space-y-3">
            {fields.map((field, index) => (
              <li
                key={field.id}
                className="grid gap-3 rounded-xl border border-slate-200 bg-slate-50/50 p-3 sm:grid-cols-[1fr_1fr_auto]"
              >
                <Field label={t('users.role')} htmlFor={`permissions.${index}.roleId`}>
                  <Controller
                    name={`permissions.${index}.roleId`}
                    control={control}
                    render={({ field: roleField }) => (
                      <Select
                        id={`permissions.${index}.roleId`}
                        ref={roleField.ref}
                        onBlur={roleField.onBlur}
                        value={roleField.value}
                        onValueChange={roleField.onChange}
                        options={roles.map((role) => ({
                          value: String(role.id),
                          label: role.name,
                        }))}
                      />
                    )}
                  />
                </Field>

                <Field label={t('users.department')} htmlFor={`permissions.${index}.departmentId`}>
                  <Controller
                    name={`permissions.${index}.departmentId`}
                    control={control}
                    render={({ field: departmentField }) => (
                      <Select
                        id={`permissions.${index}.departmentId`}
                        ref={departmentField.ref}
                        onBlur={departmentField.onBlur}
                        value={departmentField.value}
                        onValueChange={departmentField.onChange}
                        options={departments.map((department) => ({
                          value: String(department.id),
                          label: department.name,
                        }))}
                      />
                    )}
                  />
                </Field>

                <div className="flex items-end">
                  <Button
                    type="button"
                    variant="outline"
                    onClick={() => remove(index)}
                    disabled={fields.length <= 1 || isPending}
                    aria-label={t('users.removePermission')}
                  >
                    <Trash2 aria-hidden="true" />
                    <span className="sm:hidden">{t('users.removePermission')}</span>
                  </Button>
                </div>
              </li>
            ))}
          </ul>
        )}

        {permissionsRootMessage && <FieldError message={permissionsRootMessage} />}
        {permissionRows.map((permissionError, index) =>
          permissionError?.roleId?.message ? (
            <FieldError key={index} message={permissionError.roleId.message} />
          ) : null,
        )}
      </section>

      <div className="flex flex-wrap gap-3 border-t border-slate-100 pt-5">
        <Button type="submit" disabled={isPending}>
          {isPending ? t('users.saving') : submitLabel}
        </Button>
        <Button type="button" variant="outline" onClick={onCancel} disabled={isPending}>
          {t('actions.cancel')}
        </Button>
      </div>
    </form>
  )
}

function Field({
  label,
  htmlFor,
  error,
  hint,
  children,
}: {
  label: string
  htmlFor: string
  error?: string
  hint?: string
  children: ReactNode
}) {
  return (
    <div className="space-y-1.5">
      <Label htmlFor={htmlFor}>{label}</Label>
      {children}
      {hint && !error && <p className="text-xs text-slate-400">{hint}</p>}
      <FieldError message={error} />
    </div>
  )
}

function FieldError({ message }: { message?: string }) {
  const { t } = useTranslation()
  if (!message) return null
  return (
    <p role="alert" className="text-sm text-destructive">
      {t(message)}
    </p>
  )
}
