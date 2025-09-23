// Bölüm işlemleri için JavaScript kodu
document.addEventListener('DOMContentLoaded', function() {
    initializeButtons();
});

function initializeButtons() {
    // Silme butonları
    document.querySelectorAll('.btn-delete').forEach(button => {
        button.addEventListener('click', function() {
            const id = this.dataset.id;
            const name = this.dataset.name;
            deleteBolum(id, name);
        });
    });

    // Düzenleme butonları
    document.querySelectorAll('.btn-edit').forEach(button => {
        button.addEventListener('click', function() {
            const id = this.dataset.id;
            const name = this.dataset.name;
            editBolum(id, name);
        });
    });

    // Ekleme butonu
    document.getElementById('addRowBtn').addEventListener('click', function() {
        showAddForm();
    });

    // Kaydet butonu
    document.getElementById('submitBtn').addEventListener('click', function() {
        saveBolum();
    });

    // İptal butonu
    document.getElementById('cancelNewRowBtn').addEventListener('click', function() {
        hideAddForm();
    });
}

function showAddForm() {
    document.getElementById('inputRow').style.display = 'table-row';
    document.getElementById('newAd').value = '';
}

function hideAddForm() {
    document.getElementById('inputRow').style.display = 'none';
}

async function saveBolum() {
    const newAd = document.getElementById('newAd').value;
    if (!newAd) {
        showAlert('Lütfen bölüm adını giriniz', 'error');
        return;
    }

    try {
        const response = await fetch('/Bolum/Add', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ ad: newAd })
        });

        const result = await response.json();
        if (result.success) {
            showAlert('Bölüm başarıyla eklendi', 'success');
            location.reload();
        } else {
            showAlert(result.message || 'Bir hata oluştu', 'error');
        }
    } catch (error) {
        showAlert('Bir hata oluştu', 'error');
        console.error('Hata:', error);
    }
}

async function editBolum(id, currentName) {
    const newName = prompt('Yeni bölüm adını giriniz:', currentName);
    if (newName === null) return;

    try {
        const response = await fetch('/Bolum/Update', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ id: parseInt(id), ad: newName })
        });

        const result = await response.json();
        if (result.success) {
            showAlert('Bölüm başarıyla güncellendi', 'success');
            location.reload();
        } else {
            showAlert(result.message || 'Bir hata oluştu', 'error');
        }
    } catch (error) {
        showAlert('Bir hata oluştu', 'error');
        console.error('Hata:', error);
    }
}

async function deleteBolum(id, name) {
    if (!confirm(`${name} bölümünü silmek istediğinizden emin misiniz?`)) {
        return;
    }

    try {
        const response = await fetch(`/Bolum/Delete/${id}`, {
            method: 'POST'
        });

        const result = await response.json();
        if (result.success) {
            showAlert('Bölüm başarıyla silindi', 'success');
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