// Derslik işlemleri için JavaScript kodu
document.addEventListener('DOMContentLoaded', function() {
    initializeButtons();
});

function initializeButtons() {
    // Silme butonları
    document.querySelectorAll('.btn-delete').forEach(button => {
        button.addEventListener('click', function() {
            const id = this.dataset.id;
            const name = this.dataset.name;
            const kapasite = this.dataset.kapasite;
            deleteDerslik(id, name, kapasite);
        });
    });

    // Düzenleme butonları
    document.querySelectorAll('.btn-edit').forEach(button => {
        button.addEventListener('click', function() {
            const id = this.dataset.id;
            const name = this.dataset.name;
            const kapasite = this.dataset.kapasite;
            editDerslik(id, name, kapasite);
        });
    });

    // Ekleme butonu
    document.getElementById('addRowBtn').addEventListener('click', function() {
        showAddForm();
    });

    // Kaydet butonu
    document.getElementById('submitBtn').addEventListener('click', function() {
        saveDerslik();
    });

    // İptal butonu
    document.getElementById('cancelNewRowBtn').addEventListener('click', function() {
        hideAddForm();
    });
}

function showAddForm() {
    document.getElementById('inputRow').style.display = 'table-row';
    document.getElementById('newAd').value = '';
    document.getElementById('newKapasite').value = '';
}

function hideAddForm() {
    document.getElementById('inputRow').style.display = 'none';
}

async function saveDerslik() {
    const newAd = document.getElementById('newAd').value;
    const newKapasite = document.getElementById('newKapasite').value;
    
    if (!newAd || !newKapasite) {
        showAlert('Lütfen tüm alanları doldurunuz', 'error');
        return;
    }

    try {
        const response = await fetch('/Derslik/Add', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ 
                ad: newAd,
                kapasite: parseInt(newKapasite)
            })
        });

        const result = await response.json();
        if (result.success) {
            showAlert('Derslik başarıyla eklendi', 'success');
            location.reload();
        } else {
            showAlert(result.message || 'Bir hata oluştu', 'error');
        }
    } catch (error) {
        showAlert('Bir hata oluştu', 'error');
        console.error('Hata:', error);
    }
}

async function editDerslik(id, currentName, currentKapasite) {
    const newName = prompt('Yeni derslik adını giriniz:', currentName);
    if (newName === null) return;

    const newKapasite = prompt('Yeni kapasiteyi giriniz:', currentKapasite);
    if (newKapasite === null) return;

    try {
        const response = await fetch('/Derslik/Update', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ 
                id: parseInt(id),
                ad: newName,
                kapasite: parseInt(newKapasite)
            })
        });

        const result = await response.json();
        if (result.success) {
            showAlert('Derslik başarıyla güncellendi', 'success');
            location.reload();
        } else {
            showAlert(result.message || 'Bir hata oluştu', 'error');
        }
    } catch (error) {
        showAlert('Bir hata oluştu', 'error');
        console.error('Hata:', error);
    }
}

async function deleteDerslik(id, name, kapasite) {
    if (!confirm(`${name} dersliğini silmek istediğinizden emin misiniz?`)) {
        return;
    }

    try {
        const response = await fetch(`/Derslik/Delete/${id}`, {
            method: 'POST'
        });

        const result = await response.json();
        if (result.success) {
            showAlert('Derslik başarıyla silindi', 'success');
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