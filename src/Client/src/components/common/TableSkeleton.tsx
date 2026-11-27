interface TableSkeletonProps {
  rows?: number
  columns?: number
}

export function TableSkeleton({ rows = 5, columns = 4 }: TableSkeletonProps) {
  return (
    <div className="space-y-3 p-4 sm:p-5" role="status" aria-busy="true">
      {Array.from({ length: rows }, (_, rowIndex) => (
        <div
          key={rowIndex}
          className="grid gap-3"
          style={{ gridTemplateColumns: `repeat(${columns}, minmax(0, 1fr))` }}
        >
          {Array.from({ length: columns }, (_, columnIndex) => (
            <div
              key={columnIndex}
              className="h-3.5 animate-pulse rounded-md bg-slate-100"
              style={{ animationDelay: `${(rowIndex + columnIndex) * 60}ms` }}
            />
          ))}
        </div>
      ))}
    </div>
  )
}
