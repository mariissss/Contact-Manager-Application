let currentState = {
    searchTerm: '',
    sortBy: 'Id',
    isAsc: true,
    page: 1,
    pageSize: 10
};

document.addEventListener('DOMContentLoaded', () => {
    loadContactsTable();

    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') triggerSearch();
        });
    }
});

async function loadContactsTable() {
    const container = document.getElementById('tableContainer');
    const query = new URLSearchParams({
        searchTerm: currentState.searchTerm || '',
        sortBy: currentState.sortBy,
        isAsc: currentState.isAsc,
        page: currentState.page,
        pageSize: currentState.pageSize
    });

    try {
        const response = await fetch(`/Contacts/GetContactsTable?${query.toString()}`);
        if (response.ok) {
            const html = await response.text();
            container.innerHTML = html;
            updateSortIcons();
        } else {
            container.innerHTML = '<div class="alert alert-danger">Error loading contacts data.</div>';
        }
    } catch (error) {
        console.error('Failed to fetch contacts:', error);
        container.innerHTML = '<div class="alert alert-danger">Network error occurred.</div>';
    }
}

function triggerSearch() {
    const input = document.getElementById('searchInput');
    currentState.searchTerm = input.value.trim();
    currentState.page = 1; 
    loadContactsTable();
}

function sortTable(column) {
    if (currentState.sortBy === column) {
        currentState.isAsc = !currentState.isAsc;
    } else {
        currentState.sortBy = column;
        currentState.isAsc = true;
    }
    loadContactsTable();
}

function updateSortIcons() {
    document.querySelectorAll('.sort-icon').forEach(el => el.textContent = '');
    const activeIcon = document.getElementById(`sort-${currentState.sortBy}`);
    if (activeIcon) {
        activeIcon.textContent = currentState.isAsc ? ' ▲' : ' ▼';
    }
}

function loadPage(pageNumber) {
    currentState.page = pageNumber;
    loadContactsTable();
}

//inline edit
let originalRowData = {};
function enableEdit(id) {
    const row = document.querySelector(`tr[data-id="${id}"]`);
    if (!row) return;

    row.classList.add('editing-row');
    originalRowData[id] = {};

    row.querySelectorAll('.editable').forEach(cell => {
        const field = cell.getAttribute('data-field');

        if (field === 'Married') {
            const checkbox = cell.querySelector('input[type="checkbox"]');
            originalRowData[id][field] = checkbox.checked;
            checkbox.disabled = false;
        } else {
            const value = cell.innerText.trim();
            originalRowData[id][field] = value;

            const inputType = field === 'DateOfBirth' ? 'date' : (field === 'Salary' ? 'number' : 'text');
            const stepAttr = field === 'Salary' ? 'step="0.01"' : '';
            cell.innerHTML = `<input type="${inputType}" class="form-control form-control-sm" value="${value}" ${stepAttr} />`;
        }
    });

    toggleButtons(row, true);
}

function cancelEdit(id) {
    const row = document.querySelector(`tr[data-id="${id}"]`);
    if (!row) return;

    row.classList.remove('editing-row');

    row.querySelectorAll('.editable').forEach(cell => {
        const field = cell.getAttribute('data-field');
        if (field === 'Married') {
            const checkbox = cell.querySelector('input[type="checkbox"]');
            checkbox.checked = originalRowData[id][field];
            checkbox.disabled = true;
        } else {
            cell.innerText = originalRowData[id][field];
        }
    });

    toggleButtons(row, false);
    delete originalRowData[id];
}

async function saveEdit(id) {
    const row = document.querySelector(`tr[data-id="${id}"]`);
    if (!row) return;

    const updatedContact = {
        Id: id,
        Name: row.querySelector('td[data-field="Name"] input').value,
        DateOfBirth: row.querySelector('td[data-field="DateOfBirth"] input').value,
        Married: row.querySelector('td[data-field="Married"] input[type="checkbox"]').checked,
        Phone: row.querySelector('td[data-field="Phone"] input').value,
        Salary: parseFloat(row.querySelector('td[data-field="Salary"] input').value) || 0
    };

    try {
        const response = await fetch('/Contacts/UpdateContact', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(updatedContact)
        });

        if (response.ok) {
            row.classList.remove('editing-row');
            toggleButtons(row, false);
            loadContactsTable(); 
        } else {
            alert('Failed to update contact. Please check the data format.');
        }
    } catch (error) {
        console.error('Error updating:', error);
        alert('Network error while saving.');
    }
}

// 4. DELETE (Видалення запису)
async function deleteContact(id) {
    if (!confirm('Are you sure you want to delete this contact?')) return;

    try {
        const response = await fetch(`/Contacts/DeleteContact?id=${id}`, {
            method: 'DELETE'
        });

        if (response.ok) {
            loadContactsTable(); 
        } else {
            alert('Failed to delete contact.');
        }
    } catch (error) {
        console.error('Error deleting:', error);
        alert('Network error while deleting.');
    }
}

function toggleButtons(row, isEditing) {
    row.querySelector('.btn-edit').classList.toggle('d-none', isEditing);
    row.querySelector('.btn-delete').classList.toggle('d-none', isEditing);
    row.querySelector('.btn-save').classList.toggle('d-none', !isEditing);
    row.querySelector('.btn-cancel').classList.toggle('d-none', !isEditing);
}