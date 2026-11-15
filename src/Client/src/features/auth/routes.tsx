import type { AppRoute } from '@/types/router'
import { UnavailableRoute } from '@/components/common/UnavailableRoute'

export const authRoutes: AppRoute[] = [
  {
    path: 'account/password',
    element: (
      <UnavailableRoute
        titleKey="features.unavailable.passwordTitle"
        descriptionKey="features.unavailable.passwordDescription"
      />
    ),
    handle: { capability: 'modules.preview' },
  },
]
