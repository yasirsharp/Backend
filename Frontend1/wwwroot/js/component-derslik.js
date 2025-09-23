// Çift tıklama kontrolü için değişken
let isProcessing = false;
let isAddFormVisible = false;

document.addEventListener('DOMContentLoaded', function() {
    // Ekle butonu için event listener
    const addButton = document.getElementById('addRowBtn');
    if (addButton) {
        addButton.addEventListener('click', function(e) {
            e.preventDefault();
            if (!isProcessing && !isAddFormVisible) {
                handleAddRow();
            }
        });
    }
});

// Yeni satır ekleme formu göster
async function handleAddRow() {
    try {
        isProcessing = true;
        isAddFormVisible = true;

        // Varolan form varsa kaldır
        const existingForm = document.getElementById('addForm');
        if (existingForm) {
            existingForm.remove();
        }

        // Yeni form satırı oluştur
        const newRow = `
            <tr id="addForm">
                <th scope="row">#</th>
                <td><input type="text" class="form-control" id="newDerslikAd" placeholder="Derslik Adı"></td>
                <td><input type="number" class="form-control" id="newKapasite" placeholder="Kapasite"></td>
                <td>
                    <button class="btn btn-success btn-sm" onclick="handleSaveNew()">
                        <i class="bi bi-check-lg"></i>
                    </button>
                    <button class="btn btn-secondary btn-sm" onclick="cancelAddForm()">
                        <i class="bi bi-x-lg"></i>
                    </button>
                </td>
            </tr>
        `;

        // Formu tablonun en üstüne ekle
        const tbody = document.getElementById('tableBody');
        if (tbody) {
            tbody.insertAdjacentHTML('afterbegin', newRow);
            
            // Input'a odaklan
            setTimeout(() => {
                const derslikInput = document.getElementById('newDerslikAd');
                if (derslikInput) {
                    derslikInput.focus();
                }
            }, 100);
        }
    } catch (error) {
        console.error('Form ekleme hatası:', error);
        Swal.fire({
            icon: 'error',
            title: 'Hata!',
            text: 'Form eklenirken bir hata oluştu'
        });
    } finally {
        isProcessing = false;
    }
}

// Ekleme formunu iptal et
function cancelAddForm() {
    const addForm = document.getElementById('addForm');
    if (addForm) {
        addForm.remove();
    }
    isAddFormVisible = false;
}

// Tüm formları iptal et
function cancelAllForms() {
    cancelAddForm();
    window.location.reload();
}

// Yeni derslik kaydetme
async function handleSaveNew() {
    if (isProcessing) return;
    isProcessing = true;

    const ad = document.getElementById('newDerslikAd').value;
    const kapasite = document.getElementById('newKapasite').value;

    if (!validateForm(ad, kapasite)) {
        isProcessing = false;
        return;
    }

    try {
        const response = await fetch('/Derslik/Add', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ ad: ad, kapasite: parseInt(kapasite) })
        });

        const data = await response.json();
        if (data.success) {
            Swal.fire({
                icon: 'success',
                title: 'Başarılı!',
                text: 'Derslik başarıyla eklendi.',
                timer: 1500
            }).then(() => window.location.reload());
        } else {
            throw new Error(data.message);
        }
    } catch (error) {
        Swal.fire({
            icon: 'error',
            title: 'Hata!',
            text: error.message || 'Bir hata oluştu'
        });
    } finally {
        isProcessing = false;
    }
}

// Düzenleme formunu göster
function handleEdit(id, currentRow) {
    if (isProcessing) return;
    isProcessing = true;

    // Eğer zaten bir düzenleme/ekleme formu açıksa, iptal et
    cancelAllForms();

    const currentAd = currentRow.querySelector('td:nth-child(2)').textContent;
    const currentKapasite = currentRow.querySelector('td:nth-child(3)').textContent;

    // Mevcut içeriği form ile değiştir
    currentRow.innerHTML = `
        <th scope="row">${id}</th>
        <td><input type="text" class="form-control" id="editDerslikAd" value="${currentAd}"></td>
        <td><input type="number" class="form-control" id="editKapasite" value="${currentKapasite}"></td>
        <td>
            <button class="btn btn-success btn-sm" onclick="handleSaveEdit(${id})">
                <i class="bi bi-check-lg"></i>
            </button>
            <button class="btn btn-secondary btn-sm" onclick="cancelAllForms()">
                <i class="bi bi-x-lg"></i>
            </button>
        </td>
    `;

    document.getElementById('editDerslikAd').focus();
    isProcessing = false;
}

// Düzenlemeyi kaydet
async function handleSaveEdit(id) {
    if (isProcessing) return;
    isProcessing = true;

    const ad = document.getElementById('editDerslikAd').value;
    const kapasite = document.getElementById('editKapasite').value;

    if (!validateForm(ad, kapasite)) {
        isProcessing = false;
        return;
    }

    try {
        const response = await fetch('/Derslik/Update', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ id, ad, kapasite: parseInt(kapasite) })
        });

        const data = await response.json();
        if (data.success) {
            Swal.fire({
                icon: 'success',
                title: 'Başarılı!',
                text: 'Derslik başarıyla güncellendi.',
                timer: 1500
            }).then(() => window.location.reload());
        } else {
            throw new Error(data.message);
        }
    } catch (error) {
        Swal.fire({
            icon: 'error',
            title: 'Hata!',
            text: error.message || 'Bir hata oluştu'
        });
    } finally {
        isProcessing = false;
    }
}

// Silme işlemi
async function handleDelete(id, ad, kapasite) {
    if (isProcessing) return;
    
    const result = await Swal.fire({
        title: 'Emin misiniz?',
        text: `"${ad}" dersliğini silmek istediğinize emin misiniz?`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Evet, Sil!',
        cancelButtonText: 'İptal'
    });

    if (result.isConfirmed) {
        isProcessing = true;
        try {
            const response = await fetch('/Derslik/Delete', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    id: id,
                    kapasite: kapasite,
                    ad: ad
                })
            });

            const data = await response.json();
            if (data.success) {
                Swal.fire({
                    icon: 'success',
                    title: 'Başarılı!',
                    text: 'Derslik başarıyla silindi.',
                    timer: 1500
                }).then(() => window.location.reload());
            } else {
                throw new Error(data.message);
            }
        } catch (error) {
            Swal.fire({
                icon: 'error',
                title: 'Hata!',
                text: error.message || 'Bir hata oluştu'
            });
        } finally {
            window.location.reload()
            isProcessing = false;
        }
    }
}

// Form validasyonu
function validateForm(ad, kapasite) {
    if (!ad || !kapasite) {
        Swal.fire({
            icon: 'warning',
            title: 'Uyarı!',
            text: 'Lütfen tüm alanları doldurunuz.'
        });
        return false;
    }

    if (parseInt(kapasite) <= 0) {
        Swal.fire({
            icon: 'warning',
            title: 'Uyarı!',
            text: 'Kapasite 0\'dan büyük olmalıdır.'
        });
        return false;
    }

    return true;
}
