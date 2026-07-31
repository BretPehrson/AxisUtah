import {
  startTransition,
  useDeferredValue,
  useEffect,
  useMemo,
  useState,
} from 'react'
import './App.css'

type LogEntry = {
  id: number
  createdAtUtc: string
  level: string
  category: string
  eventType: string
  message: string
  source: string
  correlationId: string | null
  detailsJson: string | null
}

type SortKey =
  | 'createdAtUtc'
  | 'level'
  | 'source'
  | 'eventType'
  | 'message'

const levelOptions = ['All', 'Information', 'Warning', 'Error']
const pageSizeOptions = [25, 50, 100, 200]

function formatTimestamp(value: string) {
  const timestamp = new Date(value)

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(timestamp)
}

function App() {
  const [logs, setLogs] = useState<LogEntry[]>([])
  const [searchText, setSearchText] = useState('')
  const deferredSearchText = useDeferredValue(searchText)
  const [selectedLevel, setSelectedLevel] = useState('All')
  const [selectedSource, setSelectedSource] = useState('All')
  const [sortKey, setSortKey] = useState<SortKey>('createdAtUtc')
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('desc')
  const [take, setTake] = useState(50)
  const [activeDetailsId, setActiveDetailsId] = useState<number | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [lastUpdatedAt, setLastUpdatedAt] = useState<string | null>(null)

  useEffect(() => {
    const controller = new AbortController()

    async function loadLogs() {
      setIsLoading(true)
      setErrorMessage(null)

      try {
        const params = new URLSearchParams({ take: String(take) })

        if (selectedLevel !== 'All') {
          params.set('level', selectedLevel)
        }

        if (selectedSource !== 'All') {
          params.set('source', selectedSource)
        }

        if (deferredSearchText.trim()) {
          params.set('search', deferredSearchText.trim())
        }

        const response = await fetch(`/admin/logs?${params.toString()}`, {
          signal: controller.signal,
        })

        if (!response.ok) {
          throw new Error(`Request failed with status ${response.status}`)
        }

        const nextLogs = (await response.json()) as LogEntry[]
        startTransition(() => {
          setLogs(nextLogs)
          setLastUpdatedAt(new Date().toISOString())
        })
      } catch (error) {
        if (error instanceof DOMException && error.name === 'AbortError') {
          return
        }

        setErrorMessage(
          error instanceof Error ? error.message : 'Unable to load application logs.',
        )
      } finally {
        setIsLoading(false)
      }
    }

    void loadLogs()

    return () => controller.abort()
  }, [deferredSearchText, selectedLevel, selectedSource, take])

  const sources = useMemo(() => {
    const nextSources = Array.from(new Set(logs.map((entry) => entry.source)))

    return ['All', ...nextSources.sort((left, right) => left.localeCompare(right))]
  }, [logs])

  const sortedLogs = useMemo(() => {
    const direction = sortDirection === 'asc' ? 1 : -1

    return [...logs].sort((left, right) => {
      const leftValue = left[sortKey] ?? ''
      const rightValue = right[sortKey] ?? ''

      if (sortKey === 'createdAtUtc') {
        return (
          (new Date(String(leftValue)).getTime() - new Date(String(rightValue)).getTime()) *
          direction
        )
      }

      return String(leftValue).localeCompare(String(rightValue)) * direction
    })
  }, [logs, sortDirection, sortKey])

  function handleSort(nextSortKey: SortKey) {
    if (sortKey === nextSortKey) {
      setSortDirection((currentDirection) =>
        currentDirection === 'asc' ? 'desc' : 'asc',
      )
      return
    }

    setSortKey(nextSortKey)
    setSortDirection(nextSortKey === 'createdAtUtc' ? 'desc' : 'asc')
  }

  return (
    <main className="log-app-shell">
      <section className="log-hero">
        <div>
          <p className="eyebrow">Axis Utah Admin</p>
          <h1>Application Logs</h1>
          <p className="hero-copy">
            Search sync activity, inspect failures, and trace function runs without
            leaving the site.
          </p>
        </div>

        <div className="hero-stat-card">
          <span className="stat-label">Visible entries</span>
          <strong>{sortedLogs.length}</strong>
          <span className="stat-meta">
            {lastUpdatedAt ? `Updated ${formatTimestamp(lastUpdatedAt)}` : 'Waiting for first load'}
          </span>
        </div>
      </section>

      <section className="control-panel">
        <label className="control-group control-search">
          <span>Search</span>
          <input
            value={searchText}
            onChange={(event) => setSearchText(event.target.value)}
            placeholder="Message, event type, correlation id, or JSON details"
          />
        </label>

        <label className="control-group">
          <span>Level</span>
          <select
            value={selectedLevel}
            onChange={(event) => setSelectedLevel(event.target.value)}
          >
            {levelOptions.map((level) => (
              <option key={level} value={level}>
                {level}
              </option>
            ))}
          </select>
        </label>

        <label className="control-group">
          <span>Source</span>
          <select
            value={selectedSource}
            onChange={(event) => setSelectedSource(event.target.value)}
          >
            {sources.map((source) => (
              <option key={source} value={source}>
                {source}
              </option>
            ))}
          </select>
        </label>

        <label className="control-group">
          <span>Rows</span>
          <select
            value={String(take)}
            onChange={(event) => setTake(Number(event.target.value))}
          >
            {pageSizeOptions.map((option) => (
              <option key={option} value={option}>
                {option}
              </option>
            ))}
          </select>
        </label>
      </section>

      {errorMessage ? (
        <section className="status-banner status-error">
          <strong>Unable to load logs.</strong>
          <span>{errorMessage}</span>
        </section>
      ) : null}

      <section className="table-panel">
        <div className="table-toolbar">
          <div>
            <h2>Recent activity</h2>
            <p>Sort any column and expand rows to inspect captured JSON details.</p>
          </div>
          <div className="table-meta">
            <span>{isLoading ? 'Refreshing…' : 'Ready'}</span>
          </div>
        </div>

        <div className="log-table-wrap">
          <table className="log-table">
            <thead>
              <tr>
                <th>
                  <button type="button" onClick={() => handleSort('createdAtUtc')}>
                    Timestamp
                  </button>
                </th>
                <th>
                  <button type="button" onClick={() => handleSort('level')}>
                    Level
                  </button>
                </th>
                <th>
                  <button type="button" onClick={() => handleSort('source')}>
                    Source
                  </button>
                </th>
                <th>
                  <button type="button" onClick={() => handleSort('eventType')}>
                    Event
                  </button>
                </th>
                <th>
                  <button type="button" onClick={() => handleSort('message')}>
                    Message
                  </button>
                </th>
              </tr>
            </thead>
            <tbody>
              {sortedLogs.length === 0 && !isLoading ? (
                <tr>
                  <td className="empty-state" colSpan={5}>
                    No log entries matched the current filters.
                  </td>
                </tr>
              ) : null}

              {sortedLogs.map((entry) => {
                const isExpanded = activeDetailsId === entry.id

                return (
                  <FragmentRow
                    key={entry.id}
                    entry={entry}
                    isExpanded={isExpanded}
                    onToggle={() =>
                      setActiveDetailsId((currentId) =>
                        currentId === entry.id ? null : entry.id,
                      )
                    }
                  />
                )
              })}
            </tbody>
          </table>
        </div>
      </section>
    </main>
  )
}

type FragmentRowProps = {
  entry: LogEntry
  isExpanded: boolean
  onToggle: () => void
}

function FragmentRow({ entry, isExpanded, onToggle }: FragmentRowProps) {
  return (
    <>
      <tr className={`log-row level-${entry.level.toLowerCase()}`}>
        <td>{formatTimestamp(entry.createdAtUtc)}</td>
        <td>
          <span className={`level-pill level-${entry.level.toLowerCase()}`}>
            {entry.level}
          </span>
        </td>
        <td>{entry.source}</td>
        <td>
          <div className="event-stack">
            <strong>{entry.eventType}</strong>
            {entry.correlationId ? (
              <span className="correlation-id">{entry.correlationId}</span>
            ) : null}
          </div>
        </td>
        <td>
          <button type="button" className="message-button" onClick={onToggle}>
            <span>{entry.message}</span>
            {entry.detailsJson ? <small>{isExpanded ? 'Hide details' : 'View details'}</small> : null}
          </button>
        </td>
      </tr>
      {isExpanded && entry.detailsJson ? (
        <tr className="details-row">
          <td colSpan={5}>
            <pre>{entry.detailsJson}</pre>
          </td>
        </tr>
      ) : null}
    </>
  )
}

export default App
