// Ders işlemleri için JavaScript kodu
document.addEventListener('DOMContentLoaded', function() {
    initializeButtons();
});

function initializeButtons() {
    // Silme butonları
    document.querySelectorAll('.btn-delete').forEach(button => {
        button.addEventListener('click', function() {
            const id = this.dataset.id;
            const name = this.dataset.name;
            deleteDers(id, name);
        });
    });

    // Düzenleme butonları
    document.querySelectorAll('.btn-edit').forEach(button => {
        button.addEventListener('click', function() {
            const id = this.dataset.id;
            const name = this.dataset.name;
            editDers(id, name);
        });
    });

    // Ekleme butonu
    document.getElementById('addRowBtn').addEventListener('click', function() {
        showAddForm();
    });

    // Kaydet butonu
    document.getElementById('submitBtn').addEventListener('click', function() {
        saveDers();
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

async function saveDers() {
    const newAd = document.getElementById('newAd').value;
    if (!newAd) {
        showAlert('Lütfen ders adını giriniz', 'error');
        return;
    }

    try {
        const response = await fetch('/Ders/Add', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ ad: newAd })
        });

        const result = await response.json();
        if (result.success) {
            showAlert('Ders başarıyla eklendi', 'success');
            location.reload();
        } else {
            showAlert(result.message || 'Bir hata oluştu', 'error');
        }
    } catch (error) {
        showAlert('Bir hata oluştu', 'error');
        console.error('Hata:', error);
    }
}

async function editDers(id, currentName) {
    const newName = prompt('Yeni ders adını giriniz:', currentName);
    if (newName === null) return;

    try {
        const response = await fetch('/Ders/Update', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ id: parseInt(id), ad: newName })
        });

        const result = await response.json();
        if (result.success) {
            showAlert('Ders başarıyla güncellendi', 'success');
            location.reload();
        } else {
            showAlert(result.message || 'Bir hata oluştu', 'error');
        }
    } catch (error) {
        showAlert('Bir hata oluştu', 'error');
        console.error('Hata:', error);
    }
}

async function deleteDers(id, name) {
    if (!confirm(`${name} dersini silmek istediğinizden emin misiniz?`)) {
        return;
    }

    try {
        const response = await fetch(`/Ders/Delete/${id}`, {
            method: 'POST'
        });

        const result = await response.json();
        if (result.success) {
            showAlert('Ders başarıyla silindi', 'success');
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