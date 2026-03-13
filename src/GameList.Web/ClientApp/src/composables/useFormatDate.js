/**
 * Composable para formatear fechas en español.
 * Elimina la duplicación de formatDate en GroupPage y GameDetailModal.
 */
export function useFormatDate() {
  function formatDate(dateStr, options = { month: 'long', day: 'numeric', year: 'numeric' }) {
    return new Date(dateStr + 'T00:00:00').toLocaleDateString('es-ES', options)
  }

  function formatDateShort(dateStr) {
    return formatDate(dateStr, { month: 'short', day: 'numeric', year: 'numeric' })
  }

  return { formatDate, formatDateShort }
}
