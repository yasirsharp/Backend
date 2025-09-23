// Satıra dokununca nesne silme
function initializeDeleteButtons(entityName) {
    const deleteButtons = document.querySelectorAll('.delete-btn');
    deleteButtons.forEach(button => {
        button.addEventListener('click', function () {
            console.log("sss");
            const itemId = this.getAttribute('data-id');
            const itemName = this.getAttribute('data-name');
            var entity;
            entity = {
                id: itemId,
                ad: itemName
            };
            if (confirm(`Bu ${itemName} silmek istediğinizden emin misiniz?`)) {
                deleteItem(entity, entityName);  // Entity parametresi ile işlem yapıyoruz
            }
        });
    });
}

function deleteItem(item, entityName) {
    fetch(`/${entityName}/Delete`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(item)
    }).then(response => {
        if (response.ok) {
            showToast('success', `${entityName} başarıyla silindi!`);
            location.reload(); // Sayfayı yeniden yükle
        } else {
            showToast('danger', `${entityName} silme işlemi başarısız oldu!`);
        }
    }).catch(error => {
        console.error(`${entityName} silme sırasında hata:`, error);
        showToast('danger', 'Bir hata oluştu!');
    });
}

// Ekleme işlemi
function initializeAddButton(entityName) {
    document.getElementById("addRowBtn").addEventListener("click", function () {
        // Input satırını göster
        document.getElementById("inputRow").style.display = "table-row-group";
    });

    document.getElementById("submitBtn").addEventListener("click", function () {
        var ad = document.getElementById("newAd").value;

        if (ad) {
            var entity;
            entity = {
                ad: ad
            };

            // Fetch ile entity nesnesini backend'e gönder
            fetch(`/${entityName}/Add`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(entity) // JSON olarak gönderiyoruz
            })
                .then(response => {
                    if (response.ok) {
                        showToast('success', `${entityName} başarıyla eklendi!`);
                        location.reload(); // Sayfayı yenile
                    } else {
                        showToast('danger', `${entityName} eklenirken bir hata oluştu!`);
                    }
                })
                .catch(error => {
                    console.error("Hata:", error);
                    showToast('danger', `Bir hata oluştu: ${error.message}`);
                });
        }
        else {
            showToast('danger', 'Lütfen bir isim girin!');
        }
    });
}


// Düzenleme işlemi
function initializeEditButtons(entityName) {
    document.querySelector('#dataTable').addEventListener('click', function (event) {
        if (event.target.classList.contains('edit-btn')) {
            const button = event.target;
            const itemId = button.getAttribute('data-id');
            const row = button.closest('tr');
            const itemNameCell = row.querySelector('td:nth-child(2)');

            // İlk ad değerini alıyoruz
            const initialName = button.getAttribute('data-name');

            const input = document.createElement('input');
            input.type = 'text';
            input.classList.add('form-control');
            input.value = initialName; // İlk değer burada atanıyor

            itemNameCell.innerHTML = '';
            itemNameCell.appendChild(input);

            button.style.display = 'none';

            const saveButton = document.createElement('button');
            saveButton.classList.add('btn', 'btn-success', 'btn-sm');
            saveButton.textContent = 'Kaydet';

            const cancelButton = document.createElement('button');
            cancelButton.classList.add('btn', 'btn-danger', 'btn-sm');
            cancelButton.textContent = 'İptal';

            const actionCell = row.querySelector('td:nth-child(3)');
            actionCell.innerHTML = '';
            actionCell.appendChild(saveButton);
            actionCell.appendChild(cancelButton);

            saveButton.addEventListener('click', function () {
                // Güncel değerleri buradan alıyoruz
                var updatedName = input.value;

                var _entity = {
                    id: itemId,
                    ad: updatedName  // Güncel ad değerini buradan alıyoruz
                };

                updateItem(_entity, entityName);  // Güncel değerleri gönderiyoruz

                // Tabloyu güncelliyoruz
                itemNameCell.innerHTML = updatedName;  // Güncel ad değeri

                const newEditButton = document.createElement('button');
                newEditButton.classList.add('btn', 'btn-warning', 'btn-sm', 'edit-btn');
                newEditButton.textContent = 'Düzenle';
                newEditButton.setAttribute('data-id', itemId);
                newEditButton.setAttribute('data-name', updatedName);  // Güncel ad değeri

                const newDeleteButton = document.createElement('button');
                newDeleteButton.classList.add('btn', 'btn-danger', 'btn-sm', 'delete-btn');
                newDeleteButton.textContent = 'Sil';
                newDeleteButton.setAttribute('data-id', itemId);

                actionCell.innerHTML = '';
                actionCell.appendChild(newEditButton);
                actionCell.appendChild(newDeleteButton);

                showToast('success', `${entityName} başarıyla güncellendi!`);
            });

            cancelButton.addEventListener('click', function () {
                itemNameCell.innerHTML = initialName;  // İlk adı geri yükler

                const newEditButton = document.createElement('button');
                newEditButton.classList.add('btn', 'btn-warning', 'btn-sm', 'edit-btn');
                newEditButton.textContent = 'Düzenle';
                newEditButton.setAttribute('data-id', itemId);
                newEditButton.setAttribute('data-name', initialName);

                actionCell.innerHTML = '';
                actionCell.appendChild(newEditButton);

                showToast('danger', 'Düzenleme iptal edildi!');
            });
        }
    });
}

function updateItem(item, entityName) {
    fetch(`/${entityName}/Update`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(item)  // Burada tüm item nesnesini JSON olarak gönderiyoruz
    }).then(response => {
        if (response.ok) {
            showToast('success', `${entityName} başarıyla güncellendi!`);
        } else {
            showToast('danger', `${entityName} güncelleme işlemi başarısız oldu!`);
        }
    }).catch(error => {
        console.error(`${entityName} güncelleme sırasında hata:`, error);
        showToast('danger', 'Bir hata oluştu!');
    });
}



// Toast bildirim fonksiyonu
function showToast(type, message) {
    const toastContainer = document.getElementById('toast-container');

    const toast = document.createElement('div');
    toast.classList.add('toast', 'fade', 'show', `bg-${type}`, 'text-white');
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');
    toast.innerHTML = `<div class="toast-body">${message}</div>`;

    toastContainer.appendChild(toast);

    setTimeout(() => {
        toast.classList.remove('show');
        toast.classList.add('hide');
        setTimeout(() => toast.remove(), 300);
    }, 5000);
}
