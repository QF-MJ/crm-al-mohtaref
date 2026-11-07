# CRM المحترف: الفرونت

## التشغيل
```powershell
npm install
npm run dev
```
افتح http://localhost:5173 ثم سجّل الدخول بأي كلمة مرور واسم مستخدم من هالقائمة (بيانات وهمية MSW):
`admin` ، `salesManager` ، `salesEmployee` ، `marketingManager` ، `marketingEmployee` ، `academicAdvisor` ، `customerService` ، `employee`

## أوامر مفيدة
- `npm run lint` : فحص الكود وقواعد الحدود بين المجلدات (oxlint)
- `npm run typecheck` : فحص TypeScript
- `npm run build` : بناء نسخة نهائية
- `npm run format` : تنسيق الكود (prettier)

## قواعد العمل
اقرأ `docs/conventions.md`.

## ملاحظات
- `.env.local` للإعدادات الشخصية ولا يُرفع على git. القالب بـ `.env.example`.
- عنوان الباك `VITE_API_URL` مؤقت، اسأل فريق الباك عن المنفذ الفعلي.

## واجهة الموظفين
تستخدم ميزة الموظفين الـ API الحالي عبر Axios من `src/api/client.ts`:
- `GET /users` مع معاملات البحث والترقيم، ويعيد `{ items, page }`.
- `GET /users/{id}` لعرض تفاصيل الموظف وصلاحياته.
- `PUT /users/{id}` لتعديل بياناته وصلاحياته.
- `GET /lookups?include=Departments&include=Roles` للحصول على معرفات الأقسام والأدوار.

عنوان `/api` يضاف من إعداد `VITE_API_URL`. لا يوجد حالياً endpoint لإنشاء مستخدم في الـ API؛ لذلك يمكن إرسال نموذج الإنشاء في وضع MSW فقط، ويظل الحفظ معطلاً عند الاتصال بالـ API الحقيقي إلى أن يتوفر endpoint الإنشاء.

عند `VITE_USE_MOCKS=true` تحاكي MSW بيانات التطوير المعرفة في backend seed: مدير النظام و24 مستخدماً، مع الأقسام والأدوار والصلاحيات. عطّل هذا الخيار لاستخدام API الفعلي.
