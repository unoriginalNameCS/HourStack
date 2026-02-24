import { useState, useEffect } from "react";
import "./App.css";

const API = "/time-entries";

const emptyForm = { date: "", quantity: "", multiplier: "1", notes: "" };

function ConfirmModal({ message, onConfirm, onCancel }) {
  return (
    <div className="modal-overlay">
      <div className="modal">
        <p>{message}</p>
        <div className="modal-actions">
          <button className="btn-delete" onClick={onConfirm}>
            Delete
          </button>
          <button className="btn-secondary" onClick={onCancel}>
            Cancel
          </button>
        </div>
      </div>
    </div>
  );
}

function App() {
  const [entries, setEntries] = useState([]);
  const [form, setForm] = useState(emptyForm);
  const [editingId, setEditingId] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [confirmDelete, setConfirmDelete] = useState(null); // holds id to delete

  useEffect(() => {
    fetchEntries();
  }, []);

  async function fetchEntries() {
    setLoading(true);
    setError(null);
    try {
      const res = await fetch(API);
      if (!res.ok) throw new Error("Failed to load entries");
      setEntries(await res.json());
    } catch (e) {
      setError(e.message);
    } finally {
      setLoading(false);
    }
  }

  function handleChange(e) {
    setForm({ ...form, [e.target.name]: e.target.value });
  }

  async function handleSubmit(e) {
    e.preventDefault();
    const payload = {
      date: form.date,
      quantity: parseFloat(form.quantity),
      multiplier: parseFloat(form.multiplier) || 1,
      notes: form.notes || null,
    };
    try {
      if (editingId !== null) {
        const res = await fetch(`${API}/${editingId}`, {
          method: "PUT",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(payload),
        });
        if (!res.ok) throw new Error("Failed to update entry");
      } else {
        const res = await fetch(API, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(payload),
        });
        if (!res.ok) throw new Error("Failed to create entry");
      }
      setForm(emptyForm);
      setEditingId(null);
      fetchEntries();
    } catch (e) {
      setError(e.message);
    }
  }

  function handleEdit(entry) {
    setEditingId(entry.id);
    setForm({
      date: entry.date,
      quantity: String(entry.quantity),
      multiplier: String(entry.multiplier ?? 1),
      notes: entry.notes ?? "",
    });
  }

  function handleCancel() {
    setEditingId(null);
    setForm(emptyForm);
  }

  async function handleDelete(id) {
    setConfirmDelete(id);
  }

  async function confirmDeleteEntry() {
    const id = confirmDelete;
    setConfirmDelete(null);
    try {
      const res = await fetch(`${API}/${id}`, { method: "DELETE" });
      if (!res.ok) throw new Error("Failed to delete entry");
      fetchEntries();
    } catch (e) {
      setError(e.message);
    }
  }

  const totalHours = entries.reduce((sum, e) => sum + e.quantity, 0);
  const totalToBePaid =
    entries.reduce((sum, e) => sum + e.quantity * (e.multiplier ?? 1), 0) * 55;
  return (
    <div className="dashboard">
      {confirmDelete !== null && (
        <ConfirmModal
          message="Are you sure you want to delete this entry?"
          onConfirm={confirmDeleteEntry}
          onCancel={() => setConfirmDelete(null)}
        />
      )}
      <header className="dashboard-header">
        <h1>HourStack</h1>
        <span className="total-badge">{totalHours.toFixed(2)} hrs total</span>
        <span className="total-badge">
          ${totalToBePaid ? totalToBePaid : 0}
        </span>
      </header>

      {error && (
        <div className="error-banner">
          {error} <button onClick={() => setError(null)}>✕</button>
        </div>
      )}

      <section className="form-section">
        <h2>{editingId !== null ? "Edit Entry" : "New Entry"}</h2>
        <form onSubmit={handleSubmit} className="entry-form">
          <label>
            Date
            <input
              type="date"
              name="date"
              value={form.date}
              onChange={handleChange}
              required
            />
          </label>
          <label>
            Multiplier
            <input
              type="number"
              name="multiplier"
              value={form.multiplier}
              onChange={handleChange}
              min="1"
              step="0.5"
              placeholder="1"
              required
            />
          </label>
          <label>
            Quantity (hrs)
            <input
              type="number"
              name="quantity"
              value={form.quantity}
              onChange={handleChange}
              min="0"
              step="0.25"
              placeholder="0.00"
              required
            />
          </label>
          <label>
            Notes
            <input
              type="text"
              name="notes"
              value={form.notes}
              onChange={handleChange}
              placeholder="Optional notes"
            />
          </label>
          <div className="form-actions">
            <button type="submit" className="btn-primary">
              {editingId !== null ? "Update" : "Add Entry"}
            </button>
            {editingId !== null && (
              <button
                type="button"
                className="btn-secondary"
                onClick={handleCancel}
              >
                Cancel
              </button>
            )}
          </div>
        </form>
      </section>

      <section className="entries-section">
        <h2>Entries</h2>
        {loading ? (
          <p className="loading">Loading…</p>
        ) : entries.length === 0 ? (
          <p className="empty">No entries yet. Add one above.</p>
        ) : (
          <table className="entries-table">
            <thead>
              <tr>
                <th>Date</th>
                <th>Quantity (hours)</th>
                <th>Multipler</th>
                <th>Notes</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {entries.map((entry) => (
                <tr
                  key={entry.id}
                  className={editingId === entry.id ? "row-editing" : ""}
                >
                  <td>{entry.date}</td>
                  <td>{entry.quantity.toFixed(2)}</td>
                  <td>{entry.multiplier ?? 1}x</td>
                  <td>{entry.notes ?? "—"}</td>
                  <td className="actions-cell">
                    <button
                      className="btn-edit"
                      onClick={() => handleEdit(entry)}
                    >
                      Edit
                    </button>
                    <button
                      className="btn-delete"
                      onClick={() => handleDelete(entry.id)}
                    >
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </section>
    </div>
  );
}

export default App;
