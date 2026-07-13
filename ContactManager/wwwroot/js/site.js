let currentState = {
    searchTerm: '',
    isMarried: '',
    minSalary: '',
    maxSalary: '',
    minDob: '',
    maxDob: '',
    sortBy: 'Id',
    isAsc: true,
    page: 1,
    pageSize: 10
};

function getCurrentFileId() {
    const input = document.getElementById('currentFileId');
    return input ? parseInt(input.value) : 0;
}

document.addEventListener('DOMContentLoaded', () => {
    const fileId = getCurrentFileId();
    if (fileId > 0) {
        loadContactsTable();
        setupLiveSearchAndFilters();

        const newDobInput = document.getElementById('new-DateOfBirth');
        if (newDobInput) newDobInput.max = new Date().toISOString().split('T')[0];
    }
});

function setupLiveSearchAndFilters() {
    const searchInput = document.getElementById('searchInput');
    let debounceTimer;

    if (searchInput) {
        searchInput.addEventListener('input', (e) => {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(() => {
                currentState.searchTerm = e.target.value.trim();
                currentState.page = 1;
                loadContactsTable();
            }, 300);
        });
    }

    document.querySelectorAll('.filter-input').forEach(input => {
        input.addEventListener('input', () => {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(() => {
                currentState.isMarried = document.getElementById('filter-Married').value;
                currentState.minSalary = document.getElementById('filter-MinSalary').value;
                currentState.maxSalary = document.getElementById('filter-MaxSalary').value;
                currentState.minDob = document.getElementById('filter-MinDob').value;
                currentState.maxDob = document.getElementById('filter-MaxDob').value;
                currentState.page = 1;
                loadContactsTable();
            }, 400);
        });
    });
}

function resetFilters() {
    document.querySelectorAll('.filter-input').forEach(el => el.value = '');
    document.getElementById('searchInput').value = '';
    currentState = {
        searchTerm: '', isMarried: '', minSalary: '', maxSalary: '', minDob: '', maxDob: '',
        sortBy: 'Id', isAsc: true, page: 1, pageSize: 10
    };
    loadContactsTable();
}

async function loadContactsTable() {
    const fileId = getCurrentFileId();
    if (!fileId) return;

    const container = document.getElementById('tableContainer');
    const query = new URLSearchParams({
        fileId: fileId,
        searchTerm: currentState.searchTerm || '',
        isMarried: currentState.isMarried || '',
        minSalary: currentState.minSalary || '',
        maxSalary: currentState.maxSalary || '',
        minDob: currentState.minDob || '',
        maxDob: currentState.maxDob || '',
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

async function renameCsvFile(id, oldName) {
    let newName = prompt('Enter a new name for this CSV file:', oldName);
    if (!newName) return;
    newName = newName.trim();

    if (/^\.+$/.test(newName) || /[\\/:*?"<>|]/.test(newName)) {
        alert('Invalid file name! Avoid forbidden characters (/\\:*?"<>|) and names consisting only of dots.');
        return;
    }

    if (!newName.toLowerCase().endsWith('.csv')) {
        newName += '.csv';
    }

    if (newName === oldName) return;

    try {
        const response = await fetch('/Contacts/RenameFile', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Id: id, NewName: newName })
        });

        if (response.ok) {
            location.reload();
        } else {
            const errorText = await response.text();
            alert(`Failed to rename file: ${errorText}`);
        }
    } catch (error) {
        console.error('Error renaming:', error);
        alert('Network error while renaming.');
    }
}

async function deleteCsvFile(id) {
    if (!confirm(' Are you sure you want to delete this file? ALL contact records associated with it will be permanently deleted!')) return;

    try {
        const response = await fetch(`/Contacts/DeleteFile?id=${id}`, { method: 'DELETE' });
        if (response.ok) location.reload();
        else alert('Failed to delete file.');
    } catch (error) {
        console.error('Error deleting:', error);
    }
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

            if (field === 'DateOfBirth') {
                const today = new Date().toISOString().split('T')[0];
                cell.innerHTML = `<input type="date" class="form-control form-control-sm" value="${value}" max="${today}" />`;
            } else if (field === 'Salary') {
                const cleanSalary = value.replace(',', '.').replace(/[^0-9.]/g, '');
                cell.innerHTML = `<input type="number" class="form-control form-control-sm" value="${cleanSalary}" step="0.01" min="0" />`;
            } else {
                cell.innerHTML = `<input type="text" class="form-control form-control-sm" value="${value}" />`;
            }
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

    const nameVal = row.querySelector('td[data-field="Name"] input').value.trim();
    const dobVal = row.querySelector('td[data-field="DateOfBirth"] input').value;
    const phoneVal = row.querySelector('td[data-field="Phone"] input').value.trim();
    const salaryVal = row.querySelector('td[data-field="Salary"] input').value;
    const marriedVal = row.querySelector('td[data-field="Married"] input[type="checkbox"]').checked;

    if (!nameVal) {
        alert('Name cannot be empty!');
        return;
    }

    if (new Date(dobVal) > new Date()) {
        alert('Date of birth cannot be in the future!');
        return;
    }

    const phoneRegex = /^[\d\s()+-]+$/;
    if (!phoneRegex.test(phoneVal)) {
        alert('Phone number can only contain digits, spaces, and symbols +, -, (, )! No letters allowed.');
        return;
    }

    if (salaryVal < 0 || isNaN(salaryVal) || salaryVal === '') {
        alert('Salary must be a valid positive number!');
        return;
    }

    const updatedContact = {
        Id: id,
        CsvFileId: getCurrentFileId(),
        Name: nameVal,
        DateOfBirth: dobVal,
        Married: marriedVal,
        Phone: phoneVal,
        Salary: parseFloat(salaryVal) || 0
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
            const errorText = await response.text();
            alert(`Validation error: ${errorText}`);
        }
    } catch (error) {
        console.error('Error updating:', error);
        alert('Network error while saving.');
    }
}

async function deleteContact(id) {
    if (!confirm('Are you sure you want to delete this contact?')) return;
    try {
        const response = await fetch(`/Contacts/DeleteContact?id=${id}`, { method: 'DELETE' });
        if (response.ok) loadContactsTable();
        else alert('Failed to delete contact.');
    } catch (error) {
        console.error('Error deleting:', error);
    }
}

function toggleButtons(row, isEditing) {
    row.querySelector('.btn-edit').classList.toggle('d-none', isEditing);
    row.querySelector('.btn-delete').classList.toggle('d-none', isEditing);
    row.querySelector('.btn-save').classList.toggle('d-none', !isEditing);
    row.querySelector('.btn-cancel').classList.toggle('d-none', !isEditing);
}

async function createContact() {
    const fileId = getCurrentFileId();
    if (!fileId) return;

    const nameVal = document.getElementById('new-Name').value.trim();
    const dobVal = document.getElementById('new-DateOfBirth').value;
    const marriedVal = document.getElementById('new-Married').checked;
    const phoneVal = document.getElementById('new-Phone').value.trim();
    const salaryVal = document.getElementById('new-Salary').value;

    if (!nameVal) {
        alert(' Name cannot be empty!');
        return;
    }
    if (!dobVal) {
        alert(' Please select Date of Birth!');
        return;
    }
    if (new Date(dobVal) > new Date()) {
        alert(' Date of birth cannot be in the future!');
        return;
    }
    const phoneRegex = /^[\d\s()+-]+$/;
    if (!phoneRegex.test(phoneVal)) {
        alert(' Phone number can only contain digits, spaces, and symbols +, -, (, )! No letters allowed.');
        return;
    }
    if (salaryVal < 0 || isNaN(salaryVal) || salaryVal === '') {
        alert(' Salary must be a valid positive number!');
        return;
    }

    const newContact = {
        CsvFileId: fileId,
        Name: nameVal,
        DateOfBirth: dobVal,
        Married: marriedVal,
        Phone: phoneVal,
        Salary: parseFloat(salaryVal) || 0
    };

    try {
        const response = await fetch('/Contacts/CreateContact', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(newContact)
        });

        if (response.ok) {
            document.getElementById('addContactForm').reset();
            const modalEl = document.getElementById('addContactModal');
            const modal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
            modal.hide();
            loadContactsTable();
        } else {
            const errorText = await response.text();
            alert(` Error creating contact: ${errorText}`);
        }
    } catch (error) {
        console.error('Error creating contact:', error);
        alert('Network error while creating contact.');
    }
}