// Çift tıklama kontrolü için değişkenler
let isProcessing = false;
const DEBOUNCE_TIMEOUT = 500; // ms

document.addEventListener('DOMContentLoaded', function() {
    // Tüm işlem butonları için event listener'ları ekle
    setupButtonListeners();
});

function setupButtonListeners() {
    // Ekleme butonu
    const addButton = document.querySelector('#addDerslikButton');
    if (addButton) {
        addButton.addEventListener('click', debounce(handleAddDerslik, DEBOUNCE_TIMEOUT));
    }

    // Düzenleme butonları
    document.querySelectorAll('.edit-derslik-btn').forEach(button => {
        button.addEventListener('click', debounce(function(e) {
            handleEditDerslik(e.currentTarget.dataset.id);
        }, DEBOUNCE_TIMEOUT));
    });

    // Silme butonları
    document.querySelectorAll('.delete-derslik-btn').forEach(button => {
        button.addEventListener('click', debounce(function(e) {
            handleDeleteDerslik(e.currentTarget.dataset.id);
        }, DEBOUNCE_TIMEOUT));
    });
}

// Debounce fonksiyonu
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Derslik ekleme işlemi
async function handleAddDerslik() {
    if (isProcessing) return;
    
    try {
        isProcessing = true;
        const addButton = document.querySelector('#addDerslikButton');
        const originalText = addButton.innerHTML;
        addButton.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> İşleniyor...';
        addButton.disabled = true;

        const result = await Swal.fire({
            title: 'Yeni Derslik Ekle',
            html: `
                <input id="derslikAd" class="swal2-input" placeholder="Derslik Adı">
                <input id="derslikKapasite" type="number" class="swal2-input" placeholder="Kapasite">
            `,
            showCancelButton: true,
            confirmButtonText: 'Ekle',
            cancelButtonText: 'İptal',
            showLoaderOnConfirm: true,
            preConfirm: async () => {
                const ad = document.getElementById('derslikAd').value;
                const kapasite = document.getElementById('derslikKapasite').value;

                if (!ad || !kapasite) {
                    Swal.showValidationMessage('Lütfen tüm alanları doldurun');
                    return false;
                }

                const response = await fetch('/Derslik/Add', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: JSON.stringify({ ad, kapasite: parseInt(kapasite) })
                });

                const data = await response.json();
                if (!data.success) {
                    throw new Error(data.message);
                }
                return data;
            }
        });

        if (result.isConfirmed) {
            Swal.fire({
                title: 'Başarılı!',
                text: 'Derslik başarıyla eklendi',
                icon: 'success'
            }).then(() => {
                window.location.reload();
            });
        }
    } catch (error) {
        Swal.fire({
            title: 'Hata!',
            text: error.message || 'Bir hata oluştu',
            icon: 'error'
        });
    } finally {
        isProcessing = false;
        const addButton = document.querySelector('#addDerslikButton');
        addButton.innerHTML = 'Yeni Derslik Ekle';
        addButton.disabled = false;
    }
}

// Derslik düzenleme işlemi
async function handleEditDerslik(id) {
    if (isProcessing) return;
    
    try {
        isProcessing = true;
        const editButton = document.querySelector(`.edit-derslik-btn[data-id="${id}"]`);
        const originalText = editButton.innerHTML;
        editButton.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>';
        editButton.disabled = true;

        // Mevcut derslik bilgilerini al
        const derslikRow = editButton.closest('tr');
        const currentAd = derslikRow.querySelector('td:nth-child(2)').textContent;
        const currentKapasite = derslikRow.querySelector('td:nth-child(3)').textContent;

        const result = await Swal.fire({
            title: 'Derslik Düzenle',
            html: `
                <input id="derslikAd" class="swal2-input" placeholder="Derslik Adı" value="${currentAd}">
                <input id="derslikKapasite" type="number" class="swal2-input" placeholder="Kapasite" value="${currentKapasite}">
            `,
            showCancelButton: true,
            confirmButtonText: 'Güncelle',
            cancelButtonText: 'İptal',
            showLoaderOnConfirm: true,
            preConfirm: async () => {
                const ad = document.getElementById('derslikAd').value;
                const kapasite = document.getElementById('derslikKapasite').value;

                if (!ad || !kapasite) {
                    Swal.showValidationMessage('Lütfen tüm alanları doldurun');
                    return false;
                }

                const response = await fetch('/Derslik/Update', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: JSON.stringify({ id, ad, kapasite: parseInt(kapasite) })
                });

                const data = await response.json();
                if (!data.success) {
                    throw new Error(data.message);
                }
                return data;
            }
        });

        if (result.isConfirmed) {
            Swal.fire({
                title: 'Başarılı!',
                text: 'Derslik başarıyla güncellendi',
                icon: 'success'
            }).then(() => {
                window.location.reload();
            });
        }
    } catch (error) {
        Swal.fire({
            title: 'Hata!',
            text: error.message || 'Bir hata oluştu',
            icon: 'error'
        });
    } finally {
        isProcessing = false;
        const editButton = document.querySelector(`.edit-derslik-btn[data-id="${id}"]`);
        editButton.innerHTML = '<i class="bi bi-pencil-square"></i>';
        editButton.disabled = false;
    }
}

// Derslik silme işlemi
async function handleDeleteDerslik(id) {
    if (isProcessing) return;
    
    try {
        isProcessing = true;
        const deleteButton = document.querySelector(`.delete-derslik-btn[data-id="${id}"]`);
        const originalText = deleteButton.innerHTML;
        deleteButton.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>';
        deleteButton.disabled = true;

        const result = await Swal.fire({
            title: 'Emin misiniz?',
            text: "Bu derslik kalıcı olarak silinecek!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Evet, Sil',
            cancelButtonText: 'İptal',
            showLoaderOnConfirm: true,
            preConfirm: async () => {
                const response = await fetch('/Derslik/Delete', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: JSON.stringify(id)
                });

                const data = await response.json();
                if (!data.success) {
                    throw new Error(data.message);
                }
                return data;
            }
        });

        if (result.isConfirmed) {
            Swal.fire({
                title: 'Başarılı!',
                text: 'Derslik başarıyla silindi',
                icon: 'success'
            }).then(() => {
                window.location.reload();
            });
        }
    } catch (error) {
        Swal.fire({
            title: 'Hata!',
            text: error.message || 'Bir hata oluştu',
            icon: 'error'
        });
    } finally {
        isProcessing = false;
        const deleteButton = document.querySelector(`.delete-derslik-btn[data-id="${id}"]`);
        deleteButton.innerHTML = '<i class="bi bi-trash"></i>';
        deleteButton.disabled = false;
    }
} 