interface TableSkeletonProps {
  rows?: number
  columns?: number
}

export function TableSkeleton({ rows = 5, columns = 4 }: TableSkeletonProps) {
  return (
    <div className="space-y-2 p-4" role="status" aria-busy="true">
      {Array.from({ length: rows }, (_, rowIndex) => (
        <div
          key={rowIndex}
          className="grid gap-3"
          style={{ gridTemplateColumns: `repeat(${columns}, minmax(0, 1fr))` }}
        >
          {Array.from({ length: columns }, (_, columnIndex) => (
            <div
              key={columnIndex}
              className="h-4 animate-pulse rounded bg-slate-100"
              style={{ animationDelay: `${(rowIndex + columnIndex) * 60}ms` }}
            />
          ))}
        </div>
      ))}
    </div>
  )
}
