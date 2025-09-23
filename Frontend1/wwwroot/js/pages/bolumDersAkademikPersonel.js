// Bölüm Ders Akademik Personel işlemleri için JavaScript kodu
let selectedBolum = null;
let selectedDers = null;
let selectedAkademikPersonel = null;

document.addEventListener('DOMContentLoaded', function() {
    initializeTableListeners();
    initializeButtons();
});

function initializeTableListeners() {
    // Bölüm tablosu için dinleyici
    document.querySelectorAll('#bolumTable tbody tr').forEach(row => {
        row.addEventListener('click', function() {
            handleRowSelection(this, 'bolum');
            selectedBolum = {
                id: this.cells[0].textContent,
                ad: this.cells[1].textContent
            };
            checkSaveButtonState();
        });
    });

    // Ders tablosu için dinleyici
    document.querySelectorAll('#dersTable tbody tr').forEach(row => {
        row.addEventListener('click', function() {
            handleRowSelection(this, 'ders');
            selectedDers = {
                id: this.cells[0].textContent,
                ad: this.cells[1].textContent
            };
            checkSaveButtonState();
        });
    });

    // Akademik personel tablosu için dinleyici
    document.querySelectorAll('#personelTable tbody tr').forEach(row => {
        row.addEventListener('click', function() {
            handleRowSelection(this, 'akademikPersonel');
            selectedAkademikPersonel = {
                id: this.cells[0].textContent,
                ad: this.cells[1].textContent,
                unvan: this.cells[2].textContent
            };
            checkSaveButtonState();
        });
    });

    // Silme butonları için dinleyici
    document.querySelectorAll('.delete-btn').forEach(button => {
        button.addEventListener('click', function() {
            const id = this.dataset.id;
            deleteEslestirme(id);
        });
    });
}

function initializeButtons() {
    const saveButton = document.getElementById('saveButton');
    if (saveButton) {
        saveButton.addEventListener('click', saveEslestirme);
    }
}

function handleRowSelection(row, type) {
    // Önceki seçili satırı temizle
    document.querySelectorAll(`[data-type="${type}"]`).forEach(r => {
        r.classList.remove('selected-row');
    });
    
    // Yeni satırı seç
    row.classList.add('selected-row');

    // Tek satırı göster
    const singleRow = document.getElementById('singleRow');
    if (singleRow) {
        singleRow.style.display = 'table-row';
        updateSingleRow();
    }
}

function updateSingleRow() {
    const singleRow = document.getElementById('singleRow');
    if (singleRow) {
        const cells = singleRow.cells;
        cells[0].textContent = selectedBolum ? selectedBolum.ad : '';
        cells[1].textContent = selectedDers ? selectedDers.ad : '';
        cells[2].textContent = selectedAkademikPersonel ? 
            `${selectedAkademikPersonel.unvan} ${selectedAkademikPersonel.ad}` : '';
    }
}

function checkSaveButtonState() {
    const saveButton = document.getElementById('saveButton');
    if (saveButton) {
        saveButton.disabled = !(selectedBolum && selectedDers && selectedAkademikPersonel);
    }
    updateSingleRow();
}

async function saveEslestirme() {
    if (!selectedBolum || !selectedDers || !selectedAkademikPersonel) {
        showAlert('Lütfen tüm alanları seçin', 'error');
        return;
    }

    const eslestirme = {
        dersId: parseInt(selectedDers.id),
        bolumId: parseInt(selectedBolum.id),
        akademikPersonelId: parseInt(selectedAkademikPersonel.id)
    };

    try {
        const response = await fetch('/BolumDersAkademikPersonel/Add', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(eslestirme)
        });

        const result = await response.json();
        if (result.success) {
            showAlert('Eşleştirme başarıyla kaydedildi', 'success');
            location.reload();
        } else {
            showAlert(result.message || 'Bir hata oluştu', 'error');
        }
    } catch (error) {
        showAlert('Bir hata oluştu', 'error');
        console.error('Hata:', error);
    }
}

async function deleteEslestirme(id) {
    if (!confirm('Bu eşleştirmeyi silmek istediğinizden emin misiniz?')) {
        return;
    }

    try {
        const response = await fetch(`/BolumDersAkademikPersonel/Delete/${id}`, {
            method: 'POST'
        });

        const result = await response.json();
        if (result.success) {
            showAlert('Eşleştirme başarıyla silindi', 'success');
            location.reload();
        } else {
            showAlert(result.message || 'Bir hata oluştu', 'error');
        }
    } catch (error) {
        showAlert('Bir hata oluştu', 'error');
        console.error('Hata:', error);
    }
}

function showAlert(message, type) {
    Swal.fire({
        title: type === 'success' ? 'Başarılı!' : 'Hata!',
        text: message,
        icon: type,
        timer: 3000,
        timerProgressBar: true,
        showConfirmButton: true,
        confirmButtonText: 'Tamam',
        showDenyButton: true,
        denyButtonText: 'Yenile',
        denyButtonColor: '#3085d6',
        confirmButtonColor: '#28a745'
    }).then((result) => {
        if (result.isDenied) {
            location.reload();
        }
    });
} 