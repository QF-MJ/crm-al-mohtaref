# قواعد الفرونت

1. الميزات لا تستورد من بعضها إلا عبر `index.ts` (مثال: `@/features/clients`).
2. داخل الميزة نفسها استخدم مسارات نسبية (`./` و`../`)، لا `@/features/...`.
3. `components` و`hooks` و`lib` لا تستورد من `features`.
4. `axios` داخل `src/api` فقط. المكونات تستخدم `*.queries.ts`.
5. بيانات السيرفر = TanStack Query. Zustand للجلسة وحالة الواجهة فقط.
6. كل فورم = zod schema + React Hook Form، والنوع يُشتق من الـ schema.
7. لا نصوص عربية داخل المكونات: كلها من `src/i18n/locales`.
8. RTL: ممنوع `ml- mr- pl- pr- text-left text-right`. استخدم `ms- me- ps- pe- text-start text-end`.
9. كل المسارات من `app/router/paths.ts`، وكل راوت يحمل صلاحيته بـ `handle.permission`.

## إضافة ميزة جديدة
1. أنشئ `src/features/<name>/` بنفس قالب `auth` (api, components, hooks, pages, schemas, routes.tsx, index.ts).
2. أضف الصلاحيات الجديدة في `src/permissions/permissions.ts`.
3. أضف سطراً واحداً في `featureRoutes` بملف `src/app/router/index.tsx`.
4. أضف handlers في `src/mocks/handlers/` وسجّلها في `index.ts`.
