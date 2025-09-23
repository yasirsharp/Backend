// Akademik Personel işlemleri için JavaScript kodu
document.addEventListener('DOMContentLoaded', function() {
    initializeButtons();
});

function initializeButtons() {
    // Silme butonları
    document.querySelectorAll('.btn-delete').forEach(button => {
        button.addEventListener('click', function() {
            const id = this.dataset.id;
            const userId = this.dataset.userId;
            const name = this.dataset.name;
            const title = this.dataset.title;
            deleteAkademikPersonel(id, userId, name, title);
        });
    });

    // Düzenleme butonları
    document.querySelectorAll('.btn-edit').forEach(button => {
        button.addEventListener('click', function() {
            const id = this.dataset.id;
            const name = this.dataset.name;
            const title = this.dataset.title;
            editAkademikPersonel(id, name, title);
        });
    });

    // Ekleme butonu
    document.getElementById('addRowBtn').addEventListener('click', function() {
        showAddForm();
    });

    // Kaydet butonu
    document.getElementById('submitBtn').addEventListener('click', function() {
        saveAkademikPersonel();
    });

    // İptal butonu
    document.getElementById('cancelNewRowBtn').addEventListener('click', function() {
        hideAddForm();
    });
}

function showAddForm() {
    document.getElementById('inputRow').style.display = 'table-row';
    document.getElementById('newAd').value = '';
    document.getElementById('newUnvan').value = '';
}

function hideAddForm() {
    document.getElementById('inputRow').style.display = 'none';
}

async function saveAkademikPersonel() {
    const newAd = document.getElementById('newAd').value;
    const newUnvan = document.getElementById('newUnvan').value;
    
    if (!newAd || !newUnvan) {
        showAlert('Lütfen tüm alanları doldurunuz', 'error');
        return;
    }

    try {
        const response = await fetch('/AkademikPersonel/Add', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ 
                ad: newAd,
                unvan: newUnvan
            })
        });

        const result = await response.json();
        if (result.success) {
            showAlert('Akademik personel başarıyla eklendi', 'success');
            location.reload();
        } else {
            showAlert(result.message || 'Bir hata oluştu', 'error');
        }
    } catch (error) {
        showAlert('Bir hata oluştu', 'error');
        console.error('Hata:', error);
    }
}

async function editAkademikPersonel(id, currentName, currentUnvan) {
    const newName = prompt('Yeni adı giriniz:', currentName);
    if (newName === null) return;

    const newUnvan = prompt('Yeni unvanı giriniz:', currentUnvan);
    if (newUnvan === null) return;

    try {
        const response = await fetch('/AkademikPersonel/Update', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ 
                id: parseInt(id),
                ad: newName,
                unvan: newUnvan
            })
        });

        const result = await response.json();
        if (result.success) {
            showAlert('Akademik personel başarıyla güncellendi', 'success');
            location.reload();
        } else {
            showAlert(result.message || 'Bir hata oluştu', 'error');
        }
    } catch (error) {
        showAlert('Bir hata oluştu', 'error');
        console.error('Hata:', error);
    }
}

async function deleteAkademikPersonel(id, userId, name, title) {
    if (!confirm(`${title} ${name} akademik personelini silmek istediğinizden emin misiniz?`)) {
        return;
    }

    try {
        const response = await fetch(`/AkademikPersonel/Delete/${id}`, {
            method: 'POST'
        });

        const result = await response.json();
        if (result.success) {
            showAlert('Akademik personel başarıyla silindi', 'success');
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