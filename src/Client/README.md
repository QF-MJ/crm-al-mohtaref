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
