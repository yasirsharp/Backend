/**
 * Sınav Takvimi Yönetim Modülü
 * Bu modül, sınav programı takviminin tüm işlevselliğini yönetir.
 */

// Sabit Değerler
const CALENDAR_CONFIG = {
    SLOT_MIN_TIME: '08:00:00',
    SLOT_MAX_TIME: '18:00:00',
    SLOT_DURATION: '01:00:00'
};

// Renk üretimi için yardımcı fonksiyonlar
const COLOR_CONFIG = {
    SINAV_COLOR: '#607D8B',  // Takvimde görünen sınavlar için sabit renk
    // HSL renk değerleri için sabitler
    SATURATION: 60, // Renk doygunluğu (%)
    LIGHTNESS: 45,  // Renk parlaklığı (%)
    GOLDEN_RATIO: 0.618033988749895 // Altın oran
};

/**
 * HSL renk uzayında renk üretir
 * @param {number} index - Bölüm index'i
 * @returns {string} HSL renk kodu
 */
function generateColor(index) {
    // Altın oran ile renk üret (daha estetik dağılım için)
    const hue = (index * COLOR_CONFIG.GOLDEN_RATIO * 360) % 360;
    return `hsl(${hue}, ${COLOR_CONFIG.SATURATION}%, ${COLOR_CONFIG.LIGHTNESS}%)`;
}

/**
 * Bölüm ID'si için renk döndürür
 * @param {number} bolumId - Bölüm ID'si
 * @returns {string} Renk kodu
 */
function getBolumColor(bolumId) {
    if (bolumId === 'sinav') return COLOR_CONFIG.SINAV_COLOR;
    return generateColor(bolumId);
}

// Global değişkenler
let calendar;

/**
 * Takvim yönetimi için ana sınıf
 */
class SinavTakvimiManager {
    constructor(initialData) {
        this.isProcessing = false;
        this.dersliklerData = initialData.derslikler;
        this.dbapDetailData = initialData.dbapDetail;
        this.sinavlarData = initialData.sinavlar;
        this.akademikPersonellerData = initialData.akademikPersoneller.data;
        this.initializeComponents();
        this.setupEventListeners();
    }

    /**
     * Bileşenlerin başlangıç ayarlarını yapar
     */
    initializeComponents() {
        // Select2 inicializasyonu
        $('#mainDerslikFilter').select2({
            placeholder: "Derslik Seçiniz",
            allowClear: true,
            width: '100%',
            multiple: true
        });

        // Takvimi oluştur
        this.initializeCalendar();
    }

    /**
     * Takvimi yapılandırır ve oluşturur
     */
    initializeCalendar() {
        calendar = new FullCalendar.Calendar(document.getElementById('calendar'), {
            headerToolbar: {
                left: 'prev,next today',
                center: 'title',
                right: 'exportPdfButton exportExcelButton dayGridMonth,timeGridWeek,timeGridDay'
            },
            customButtons: {
                exportExcelButton: {
                    text: 'Excel',
                    click: () => this.exportToExcel()
                },
                exportPdfButton: {
                    text: 'Pdf',
                    click: () => this.exportToPDF()
                }
            },
            initialView: 'timeGridWeek',
            locale: 'tr',
            height: '100%',
            editable: true,
            droppable: true,
            slotMinTime: CALENDAR_CONFIG.SLOT_MIN_TIME,
            slotMaxTime: CALENDAR_CONFIG.SLOT_MAX_TIME,
            allDaySlot: false,
            slotDuration: CALENDAR_CONFIG.SLOT_DURATION,
            snapDuration: CALENDAR_CONFIG.SLOT_DURATION,
            expandRows: true,
            slotEventOverlap: false,
            nowIndicator: true,

            // Sürükle-Bırak ayarları
            eventDrop: (info) => {
                this.handleEventDrop(info);
            },
            drop: (info) => {
                this.handleExternalDrop(info);
            },
            eventReceive: (info) => {
                this.handleEventReceive(info);
            },
            // Sınav detay görüntüleme için tıklama olayı
            eventClick: (info) => {
                this.handleEventClick(info);
            }


        });

        // Dış etkinlikleri sürüklenebilir yap
        this.initializeExternalEvents();

        calendar.render();
        this.loadSinavlar();
    }

    /**
     * Event listener'ları ayarlar
     */
    setupEventListeners() {
        // Derslik seçimi değiştiğinde takvim güncelleme özelliği kaldırıldı
    }

    /**
     * Sınavları yükler ve takvime ekler
     */
    loadSinavlar() {
        // Önce tüm eventları temizle
        calendar.removeAllEvents();

        try {
            // Veri kontrolü
            if (!this.sinavlarData) {
                console.warn('Sınav verisi bulunamadı');
                return;
            }

            console.log(this.sinavlarData);

            // Veriyi işle
            if (Array.isArray(this.sinavlarData)) {
                this.sinavlarData.forEach(sinav => {
                    if (sinav) {
                        this.createCalendarEvent(sinav);
                    }
                });
            } else {
                console.warn('Sınav verisi dizi değil:', this.sinavlarData);
            }

            // Takvimi güncelle
            calendar.gotoDate(new Date());
            calendar.render();
        } catch (error) {
            console.error('Sınav yükleme hatası:', error);
            this.handleError(error);
        }
    }

    /**
     * Tek bir sınav için takvim eventi oluşturur
     * @param {Object} sinav - Sınav verisi
     */
    createCalendarEvent(sinav) {
        try {
            // Null kontrolü
            if (!sinav) {
                console.warn('Geçersiz sınav verisi:', sinav);
                return;
            }

            // Tarih kontrolü
            if (!sinav.sinavTarihi) {
                console.warn('Sınav tarihi bulunamadı:', sinav);
                return;
            }

            // Tarih ve saat bilgisini birleştir
            let eventDate;
            try {
                eventDate = new Date(sinav.sinavTarihi);
                if (isNaN(eventDate.getTime())) {
                    console.warn('Geçersiz tarih formatı:', sinav.sinavTarihi);
                    return;
                }
            } catch (error) {
                console.error('Tarih dönüştürme hatası:', error);
                return;
            }

            // Saat bilgisi kontrolü
            if (sinav.sinavBaslangicSaati) {
                const [hours, minutes] = sinav.sinavBaslangicSaati.split(':');
                eventDate.setHours(parseInt(hours), parseInt(minutes), 0);
            }

            // Derslik bilgisini bul
            const derslik = this.dersliklerData.find(d => d.id === sinav.derslikId);

            // Gözetmen bilgisini bul
            const gozetmenId = sinav.gozetmenId || null;
            console.log(sinav.dersBolumAkademikPersonelId);
            // Event oluştur
            const event = {
                id: sinav.id,
                title: sinav.dersAd || 'İsimsiz Sınav',
                start: eventDate,
                description: sinav.akademikPersonelAd ? `${sinav.unvan || ''} ${sinav.akademikPersonelAd}` : '',
                backgroundColor: COLOR_CONFIG.SINAV_COLOR,
                borderColor: COLOR_CONFIG.SINAV_COLOR,
                textColor: '#fff',
                allDay: false,
                extendedProps: {
                    dersAd: sinav.dersAd || '',
                    akademikPersonelAd: sinav.akademikPersonelAd || '',
                    bolumAd: sinav.bolumAd || '',
                    unvan: sinav.unvan || '',
                    derslikId: sinav.derslikId,
                    derslikAd: derslik ? derslik.ad : 'Derslik Atanmamış',
                    derslikKontenjan: sinav.derslikKontenjan,
                    gozetmenId: gozetmenId,
                    derBolumAkademikPersonelId: sinav.dersBolumAkademikPersonelId,
                    dbapId: sinav.dersBolumAkademikPersonelId
                }
            };

            calendar.addEvent(event);
        } catch (error) {
            console.error('Event oluşturma hatası:', error, 'Sınav:', sinav);
        }
    }

    /**
     * Hata yönetimi
     * @param {Error} error - Hata nesnesi
     */
    handleError(error) {
        Swal.fire({
            title: 'Hata!',
            text: error.message,
            icon: 'error'
        });
    }

    /**
     * Dış etkinlikleri sürüklenebilir yapar
     */
    initializeExternalEvents() {
        const draggableEvents = document.querySelectorAll('.external-event');
        draggableEvents.forEach(eventEl => {
            new FullCalendar.Draggable(eventEl, {
                eventData: (eventEl) => ({
                    title: eventEl.querySelector('h5').innerText,
                    description: eventEl.querySelector('p').innerText,
                    backgroundColor: getBolumColor(parseInt(eventEl.dataset.bolumId)),
                    borderColor: getBolumColor(parseInt(eventEl.dataset.bolumId)),
                    textColor: '#fff',
                    extendedProps: {
                        derBolumAkademikPersonelId: parseInt(eventEl.dataset.id),
                        dersId: parseInt(eventEl.dataset.dersId),
                        bolumId: parseInt(eventEl.dataset.bolumId)
                    }
                })
            });
        });
    }

    /**
     * Takvim içindeki event sürüklendiğinde
     */
    async handleEventDrop(info) {
        if (this.isProcessing) return;
        this.isProcessing = true;

        try {
            const event = info.event;
            const newDate = event.start;
            const extendedProps = event.extendedProps;
            const oldDate = info.oldEvent.start;

            // Mevcut derslik ve gözetmen bilgilerini al
            //const response = await fetch(`/SinavDetay/GetSinavDerslikler/${event.id}`);
            //const response = {}
            //const derslikData = await response.json();

            //if (!derslikData.success) {
            //    info.revert();
            //    throw new Error(derslikData.message);
            //}

            // Bitiş saatini başlangıç saatinden 1 saat sonrası olarak ayarla
            const baslangicSaat = newDate.getHours();
            const baslangicDakika = newDate.getMinutes();
            const bitisSaat = baslangicSaat + 1;

            // Gözetmen seçimi için select listesi HTML'i oluştur
            const gozetmenSelectHTML = (selectedGozetmenId = null) => `
                <select class="form-control gozetmen-select">
                    <option value="">Gözetmen Seçiniz</option>
                    ${this.akademikPersonellerData.map(ap => 
                        `<option value="${ap.id}" ${ap.id === selectedGozetmenId ? 'selected' : ''}>${ap.unvan} ${ap.ad}</option>`
                    ).join('')}
                </select>
            `;

            // Eski derslik bilgilerini hazırla
            /*const eskiDerslikBilgileri = derslikData.data.map(d => {
                const derslik = this.dersliklerData.find(dr => dr.id === d.derslikId);
                const gozetmen = this.akademikPersonellerData.find(ap => ap.id === d.gozetmenId);
                return {
                    derslik: derslik ? derslik.ad : 'Bilinmiyor',
                    kapasite: derslik ? derslik.kapasite : '-',
                    gozetmen: gozetmen ? `${gozetmen.unvan} ${gozetmen.ad}` : 'Atanmamış'
                };
            });*/

            // Seçili yeni derslikleri al
            const yeniDerslikIds = $('#mainDerslikFilter').val();
            const yeniDerslikler = yeniDerslikIds.map(id => this.dersliklerData.find(d => d.id === parseInt(id)));

            // Onay penceresi için HTML hazırla
            const confirmHTML = `
                <div class="text-start">
                    <h5 class="mb-3">Sınav Bilgileri:</h5>
                    <div class="mb-2">
                        <strong>Ders:</strong> ${extendedProps.dersAd}
                    </div>
                    <div class="mb-2">
                        <strong>Öğretim Görevlisi:</strong> ${extendedProps.unvan} ${extendedProps.akademikPersonelAd}
                    </div>
                    <div class="mb-2">
                        <strong>Bölüm:</strong> ${extendedProps.bolumAd}
                    </div>
                    <hr>
                    <h5 class="mb-3">Değişiklik Detayları:</h5>
                    <div class="mb-2">
                        <strong>Eski Tarih/Saat:</strong> ${oldDate.toLocaleDateString('tr-TR')} ${oldDate.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })}
                    </div>
                    <div class="mb-2">
                        <strong>Yeni Tarih/Saat:</strong> ${newDate.toLocaleDateString('tr-TR')} ${newDate.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })}
                    </div>
                    <div class="mb-2">
                        <strong>Yeni Bitiş Saati:</strong> ${bitisSaat.toString().padStart(2, '0')}:${baslangicDakika.toString().padStart(2, '0')}
                    </div>
                    <hr>
                    <h5 class="mb-3">Mevcut Derslik ve Gözetmen Bilgileri:</h5>
                    {eskiDerslikBilgileri.map((d, index) => 
                        <div class="mb-2">
                            <strong>Derslik {index + 1}:</strong> {d.derslik} (Kapasite: {d.kapasite})
                            <br>
                            <strong>Gözetmen:</strong> {d.gozetmen}
                        </div>
                    ).join('')}
                    <hr>
                    <h5 class="mb-3">Yeni Derslik ve Gözetmen Atamaları:</h5>
                    <div id="yeniDerslikContainer">
                        ${yeniDerslikler.map((derslik, index) => `
                            <div class="mb-3 derslik-gozetmen-item" data-derslik-id="${derslik.id}">
                                <div class="mb-2">
                                    <strong>Derslik ${index + 1}:</strong> ${derslik.ad} (Kapasite: ${derslik.kapasite})
                                </div>
                                <div>
                                    <label>Gözetmen Seç:</label>
                                    ${gozetmenSelectHTML()}
                                </div>
                            </div>
                        `).join('')}
                    </div>
                </div>
            `;

            // Onay penceresi göster
            const result = await Swal.fire({
                title: 'Sınav Taşıma Onayı',
                html: confirmHTML,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Evet, Taşı',
                cancelButtonText: 'İptal',
                width: '800px',
                didOpen: () => {
                    // Select2'yi aktifleştir
                    $('.gozetmen-select').select2({
                        dropdownParent: $('.swal2-container'),
                        width: '100%',
                        placeholder: "Gözetmen Seçiniz"
                    });
                },
                preConfirm: () => {
                    // Seçilen gözetmenleri topla
                    const yeniDerslikler = [];
                    document.querySelectorAll('.derslik-gozetmen-item').forEach(item => {
                        yeniDerslikler.push({
                            derslikId: parseInt(item.dataset.derslikId),
                            gozetmenId: parseInt(item.querySelector('.gozetmen-select').value) || null
                        });
                    });
                    return yeniDerslikler;
                }
            });

            if (!result.isConfirmed) {
                info.revert();
                return;
            }

            // Güncelleme verilerini hazırla
            const updateData = {
                id: parseInt(event.id),
                dbapId: parseInt(event.extendedProps.derBolumAkademikPersonelId || event.extendedProps.dbapId),
                sinavTarihi: newDate.toISOString().split('T')[0],
                sinavBaslangicSaati: `${baslangicSaat.toString().padStart(2, '0')}:${baslangicDakika.toString().padStart(2, '0')}:00`,
                sinavBitisSaati: `${bitisSaat.toString().padStart(2, '0')}:${baslangicDakika.toString().padStart(2, '0')}:00`,
                derslikler: result.value.map(d => ({
                    derslikId: d.derslikId,
                    gozetmenId: d.gozetmenId || 0
                }))
            };

            // Debug için
            console.log('Derslikler:', result.value);
            console.log('Event Extended Props:', extendedProps);
            console.log('Update Data:', updateData);


            // Güncelleme isteği gönder
            fetch('/SinavDetay/Update', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify(updateData)
            })
                .then(response => response.json())
                .then(result => {
                    if (result.success) {
                        Swal.fire('Başarılı!', result.message, 'success');
                        // Takvimi yenile
                        this.loadSinavlar();
                    } else {
                        info.revert();
                        throw new Error(result.message);
                    }
                })
                .catch(error => {
                    console.log(error);
                    Swal.fire('Hata!', 'Taşıma işlemi sırasında bir hata oluştu' + error, 'error');
                });


            /*if (!updateResponse.ok) {
                console.log(updateResponse.result);
                throw new Error("Bir hata var!");
            }
            const updateResult = await updateResponse.json();

            if (!updateResult.success) {
                info.revert();
                throw new Error(updateResult.message);
            }

            // Başarılı güncelleme mesajı göster
            await Swal.fire({
                title: 'Başarılı!',
                text: 'Sınav başarıyla taşındı.',
                icon: 'success'
            });*/

        } catch (error) {
            info.revert();
            console.log(error);
            this.handleError(error);
        } finally {
            this.isProcessing = false;
        }
    }

    /**
     * Takvimde bir sınava tıklandığında
     */
    handleEventClick(info) {
        const event = info.event;
        const extendedProps = event.extendedProps;
        const eventDate = event.start;
        const formattedDate = eventDate.toLocaleDateString('tr-TR');
        const formattedTime = eventDate.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' });

        // Derslik bilgisini bul
        const derslik = this.dersliklerData.find(d => d.id === extendedProps.derslikId);

        // Gözetmen bilgisini bul
        const gozetmen = this.akademikPersonellerData.find(ap => ap.id === extendedProps.gozetmenId);
        const gozetmenBilgisi = gozetmen ? `${gozetmen.ad} (${gozetmen.unvan})` : '-';

        const derslikBilgisi = derslik 
            ? `${derslik.ad} (Kapasite: ${derslik.kapasite} kişi)`
            : 'Derslik Atanmamış';

        const detayHTML = `
            <div style="text-align: left; margin-top: 10px;">
                <h4>Ders Bilgileri:</h4>
                <p><strong>Ders:</strong> ${extendedProps.dersAd}</p>
                <p><strong>Öğretim Görevlisi:</strong> ${extendedProps.unvan} ${extendedProps.akademikPersonelAd}</p>
                <p><strong>Bölüm:</strong> ${extendedProps.bolumAd}</p>
                <hr>
                <h4>Sınav Zamanı:</h4>
                <p><strong>Tarih:</strong> ${formattedDate}</p>
                <p><strong>Saat:</strong> ${formattedTime}</p>
                <hr>
                <h4>Derslik ve Gözetmen Bilgisi:</h4>
                <p><strong>Derslik:</strong> ${derslikBilgisi}</p>
                <p><strong>Gözetmen:</strong> ${gozetmenBilgisi}</p>
            </div>
        `;

        Swal.fire({
            title: 'Sınav Detayları',
            html: detayHTML,
            icon: 'info',
            showCancelButton: true,
            showDenyButton: true,
            confirmButtonText: 'Düzenle',
            denyButtonText: 'Sil',
            cancelButtonText: 'Kapat',
            confirmButtonColor: '#3085d6',
            denyButtonColor: '#d33',
            cancelButtonColor: '#6c757d'
        }).then((result) => {
            if (result.isConfirmed) {
                this.showEditDialog(event);
            } else if (result.isDenied) {
                this.showDeleteConfirmation(event);
            }
        });
    }

    /**
     * Sınav silme onay dialogu
     */
    showDeleteConfirmation(event) {
        Swal.fire({
            title: 'Emin misiniz?',
            text: "Bu sınavı silmek istediğinize emin misiniz?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Evet, Sil',
            cancelButtonText: 'İptal'
        }).then((result) => {
            if (result.isConfirmed) {
                this.deleteSinav(event.id);
            }
        });
    }

    /**
     * Sınavı sil
     */
    deleteSinav(sinavId) {
        fetch('/SinavDetay/Delete', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify(sinavId)
        })
        .then(response => response.json())
        .then(result => {
            if (result.success) {
                Swal.fire('Başarılı!', result.message, 'success');
                this.loadSinavlar();
            } else {
                Swal.fire('Hata!', result.message, 'error');
            }
        })
        .catch(error => {
            Swal.fire('Hata!', 'Silme işlemi sırasında bir hata oluştu', 'error');
        });
    }

    /**
     * Düzenleme dialogunu göster
     */
    showEditDialog(event) {
        const props = event.extendedProps;
        const eventDate = event.start;

        // Mevcut derslik-gözetmen eşleştirmelerini al
        fetch(`/SinavDetay/GetSinavDerslikler/${event.id}`)
            .then(response => response.json())
            .then(result => {
                if (result.success) {
                    const mevcutDerslikler = result.data;

                    // Gözetmen seçimi için select listesi HTML'i oluştur
                    const gozetmenSelectHTML = (selectedGozetmenId = null) => `
                        <select class="form-control gozetmen-select">
                            <option value="">Gözetmen Seçiniz</option>
                            ${this.akademikPersonellerData.map(ap => 
                                `<option value="${ap.id}" ${ap.id === selectedGozetmenId ? 'selected' : ''}>${ap.unvan} ${ap.ad}</option>`
                            ).join('')}
                        </select>
                    `;

                    // Derslik seçimi için mevcut derslikleri al
                    const derslikSelectHTML = `
                        <select class="form-control derslik-select" multiple>
                            ${this.dersliklerData.map(d => 
                                `<option value="${d.id}" ${mevcutDerslikler.some(md => md.derslikId === d.id) ? 'selected' : ''}>
                                    ${d.ad} (Kapasite: ${d.kapasite})
                                </option>`
                            ).join('')}
                        </select>
                    `;

                    const editHTML = `
                        <div style="text-align: left; margin-top: 10px;">
                            <div class="form-group mb-3">
                                <label>Saat:</label>
                                <input type="time" class="form-control" id="sinavSaati" value="${eventDate.toTimeString().slice(0,5)}">
                            </div>
                            <div class="form-group mb-3">
                                <label>Derslikler:</label>
                                ${derslikSelectHTML}
                            </div>
                            <div id="gozetmenContainer">
                                ${mevcutDerslikler.map(md => {
                                    const derslik = this.dersliklerData.find(d => d.id === md.derslikId);
                                    return `
                                        <div class="form-group mb-3 derslik-gozetmen-item" data-derslik-id="${md.derslikId}">
                                            <label>${derslik.ad} için Gözetmen:</label>
                                            ${gozetmenSelectHTML(md.gozetmenId)}
                                        </div>
                                    `;
                                }).join('')}
                            </div>
                        </div>
                    `;

                    Swal.fire({
                        title: 'Sınavı Düzenle',
                        html: editHTML,
                        width: '600px',
                        showCancelButton: true,
                        confirmButtonText: 'Kaydet',
                        cancelButtonText: 'İptal',
                        didOpen: () => {
                            // Select2'yi aktifleştir
                            $('.derslik-select').select2({
                                dropdownParent: $('.swal2-container'),
                                width: '100%',
                                placeholder: "Derslik Seçiniz"
                            });

                            // Mevcut gözetmen seçimlerini aktifleştir
                            $('.gozetmen-select').select2({
                                dropdownParent: $('.swal2-container'),
                                width: '100%',
                                placeholder: "Gözetmen Seçiniz"
                            });

                            // Derslik seçimi değiştiğinde
                            $('.derslik-select').on('change', (e) => {
                                const selectedDerslikler = $(e.target).val();
                                const container = document.getElementById('gozetmenContainer');
                                container.innerHTML = '';

                                selectedDerslikler.forEach(derslikId => {
                                    const derslik = this.dersliklerData.find(d => d.id === parseInt(derslikId));
                                    // Mevcut gözetmen seçimini koru
                                    const mevcutDerslik = mevcutDerslikler.find(md => md.derslikId === parseInt(derslikId));
                                    container.innerHTML += `
                                        <div class="form-group mb-3 derslik-gozetmen-item" data-derslik-id="${derslikId}">
                                            <label>${derslik.ad} için Gözetmen:</label>
                                            ${gozetmenSelectHTML(mevcutDerslik?.gozetmenId)}
                                        </div>
                                    `;
                                });

                                // Yeni eklenen select2'leri aktifleştir
                                $('.gozetmen-select').select2({
                                    dropdownParent: $('.swal2-container'),
                                    width: '100%',
                                    placeholder: "Gözetmen Seçiniz"
                                });
                            });
                        },
                        preConfirm: () => {
                            const saat = document.getElementById('sinavSaati').value;
                            const [baslangicSaat, baslangicDakika] = saat.split(':').map(Number);
                            const bitisSaat = baslangicSaat + 1;
                            
                            const derslikler = [];
                            document.querySelectorAll('.derslik-gozetmen-item').forEach(item => {
                                const gozetmenId = parseInt(item.querySelector('.gozetmen-select').value);
                                derslikler.push({
                                    derslikId: parseInt(item.dataset.derslikId),
                                    gozetmenId: isNaN(gozetmenId) ? null : gozetmenId
                                });
                            });

                            const updateData = {
                                id: event.id,
                                dbapId: parseInt(event.extendedProps.derBolumAkademikPersonelId || event.extendedProps.dbapId),
                                sinavTarihi: eventDate.toISOString().split('T')[0],
                                sinavBaslangicSaati: `${baslangicSaat.toString().padStart(2, '0')}:${baslangicDakika.toString().padStart(2, '0')}:00`,
                                sinavBitisSaati: `${bitisSaat.toString().padStart(2, '0')}:${baslangicDakika.toString().padStart(2, '0')}:00`,
                                derslikler: derslikler
                            };

                            return updateData;
                        }
                    }).then((result) => {
                        if (result.isConfirmed) {
                            this.updateSinav(result.value);
                        }
                    });
                }
            });
    }

    /**
     * Sınavı güncelle
     */
    updateSinav(data) {
        fetch('/SinavDetay/Update', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify(data)
        })
        .then(response => response.json())
        .then(result => {
            if (result.success) {
                Swal.fire('Başarılı!', result.message, 'success');
                this.loadSinavlar();
            } else {
                Swal.fire('Hata!', result.message, 'error');
            }
        })
        .catch(error => {
            Swal.fire('Hata!', 'Güncelleme sırasında bir hata oluştu', 'error');
        });
    }

    /**
     * Dışarıdan sürüklenen event takvime bırakıldığında
     */
    handleExternalDrop(info) {
        const droppedEl = info.draggedEl;
        const dropDate = info.date;

        // Seçili derslikleri kontrol et
        const selectedDerslikIds = $('#mainDerslikFilter').val();
        if (!selectedDerslikIds || selectedDerslikIds.length === 0) {
            Swal.fire({
                title: 'Hata!',
                text: 'Lütfen en az bir derslik seçin',
                icon: 'error'
            });
            return false;
        }

        // Seçili dersliklerin detaylarını al
        const selectedDerslikler = selectedDerslikIds.map(id => 
            this.dersliklerData.find(d => d.id === parseInt(id))
        ).filter(d => d !== undefined);

        // Gözetmen seçimi için HTML oluştur
        const derslikGozetmenHTML = selectedDerslikler.map(derslik => `
            <div class="form-group mb-3 derslik-gozetmen-group">
                <label class="fw-bold">${derslik.ad} (Kapasite: ${derslik.kapasite})</label>
                <select class="form-control gozetmen-select" data-derslik-id="${derslik.id}">
                    <option value="">Gözetmen Seçiniz</option>
                    ${this.akademikPersonellerData.map(ap => 
                        `<option value="${ap.id}">${ap.unvan} ${ap.ad}</option>`
                    ).join('')}
                </select>
            </div>
        `).join('');

        // Ders bilgilerini al
        const dersAd = droppedEl.querySelector('h5').innerText;
        const akademikPersonel = droppedEl.querySelector('p').innerText;

        // SweetAlert2 ile form göster
        Swal.fire({
            title: 'Sınav Detayları',
            html: `
                <div class="text-start">
                    <div class="mb-4">
                        <h5>Ders Bilgileri:</h5>
                        <p class="mb-1"><strong>Ders:</strong> ${dersAd}</p>
                        <p class="mb-1"><strong>Öğretim Görevlisi:</strong> ${akademikPersonel}</p>
                        <p class="mb-1"><strong>Tarih:</strong> ${dropDate.toLocaleDateString('tr-TR')}</p>
                        <p class="mb-1"><strong>Saat:</strong> ${dropDate.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })}</p>
                    </div>
                    <hr>
                    <h5>Derslik ve Gözetmen Atamaları:</h5>
                    ${derslikGozetmenHTML}
                </div>
            `,
            showCancelButton: true,
            confirmButtonText: 'Kaydet',
            cancelButtonText: 'İptal',
            width: '600px',
            didOpen: () => {
                // Select2'yi aktifleştir
                $('.gozetmen-select').select2({
                    dropdownParent: $('.swal2-container'),
                    width: '100%',
                    placeholder: "Gözetmen Seçiniz"
                });
            },
            preConfirm: () => {
                // Form verilerini topla
                const derslikler = [];
                $('.derslik-gozetmen-group').each(function() {
                    const derslikId = $(this).find('.gozetmen-select').data('derslik-id');
                    const gozetmenId = $(this).find('.gozetmen-select').val();
                    derslikler.push({
                        derslikId: parseInt(derslikId),
                        gozetmenId: gozetmenId ? parseInt(gozetmenId) : null
                    });
                });

                // Sınav verisi oluştur
                return {
                    derBolumAkademikPersonelId: parseInt(droppedEl.dataset.id),
                    sinavTarihi: dropDate.toISOString().split('T')[0],
                    sinavBaslangicSaati: `${dropDate.getHours().toString().padStart(2, '0')}:${dropDate.getMinutes().toString().padStart(2, '0')}:00`,
                    sinavBitisSaati: `${(dropDate.getHours() + 1).toString().padStart(2, '0')}:${dropDate.getMinutes().toString().padStart(2, '0')}:00`,
                    derslikler: derslikler
                };
            }
        }).then((result) => {
            if (result.isConfirmed) {
                // API isteği gönder
                const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
                
                fetch('/SinavDetay/Add', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': token
                    },
                    body: JSON.stringify(result.value)
                })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        Swal.fire({
                            title: 'Başarılı!',
                            text: data.message,
                            icon: 'success'
                        });
                        this.loadSinavlar();
                    } else {
                        throw new Error(data.message);
                    }
                })
                .catch(error => {
                    Swal.fire({
                        title: 'Hata!',
                        text: error.message,
                        icon: 'error'
                    });
                });
            }
        });

        return false;
    }

    /**
     * Yeni event takvime eklendiğinde
     */
    async handleEventReceive(info) {
        const event = info.event;
        console.log('Yeni event alındı:', event);
        
        // Eğer bu bir güncelleme ise ve event'in ID'si varsa
        if (event.id) {
            const baslangicSaat = event.start.getHours();
            const baslangicDakika = event.start.getMinutes();
            const bitisSaat = baslangicSaat + 1;

            const updateData = {
                id: parseInt(event.id),
                dbapId: parseInt(event.extendedProps.derBolumAkademikPersonelId || event.extendedProps.dbapId),
                sinavTarihi: event.start.toISOString().split('T')[0],
                sinavBaslangicSaati: `${baslangicSaat.toString().padStart(2, '0')}:${baslangicDakika.toString().padStart(2, '0')}:00`,
                sinavBitisSaati: `${bitisSaat.toString().padStart(2, '0')}:${baslangicDakika.toString().padStart(2, '0')}:00`,
                derslikler: $('#mainDerslikFilter').val().map(derslikId => ({
                    derslikId: parseInt(derslikId),
                    gozetmenId: null
                }))
            };

            try {
                const response = await fetch('/SinavDetay/Update', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: JSON.stringify(updateData)
                });

                const result = await response.json();
                if (!result.success) {
                    throw new Error(result.message);
                }

                // Başarılı güncelleme sonrası takvimi yenile
                this.loadSinavlar();
            } catch (error) {
                this.handleError(error);
                event.remove();
            }
        }
    }

    /**
     * Takvim verilerini PDF'e aktarır
     */
    exportToPDF() {
        try {
            // Font tanımlaması
            pdfMake.fonts = {
                Roboto: {
                    normal: 'https://cdnjs.cloudflare.com/ajax/libs/pdfmake/0.2.7/fonts/Roboto/Roboto-Regular.ttf',
                    bold: 'https://cdnjs.cloudflare.com/ajax/libs/pdfmake/0.2.7/fonts/Roboto/Roboto-Medium.ttf',
                    italics: 'https://cdnjs.cloudflare.com/ajax/libs/pdfmake/0.2.7/fonts/Roboto/Roboto-Italic.ttf',
                    bolditalics: 'https://cdnjs.cloudflare.com/ajax/libs/pdfmake/0.2.7/fonts/Roboto/Roboto-MediumItalic.ttf'
                }
            };

            // Sınav verilerini topla ve grupla
            const sinavMap = new Map(); // Sınavları ID'lerine göre gruplamak için
            const events = calendar.getEvents();

            // Önce tüm sınavları topla
            events.forEach(event => {
                const props = event.extendedProps;
                const derslik = this.dersliklerData.find(d => d.id === props.derslikId);
                const gozetmen = this.akademikPersonellerData.find(ap => ap.id === props.gozetmenId);

                const derslikBilgisi = derslik ? 
                    `${derslik.ad}\n(${gozetmen ? `${gozetmen.unvan} ${gozetmen.ad}` : 'Gözetmen Atanmamış'})` : 
                    'Derslik Atanmamış';

                // Sınavı haritaya ekle
                sinavMap.set(event.id, {
                    dersAdi: props.dersAd,
                    ogretimUyesi: `${props.unvan} ${props.akademikPersonelAd}`,
                    sinavTarihi: event.start.toLocaleDateString('tr-TR'),
                    saatAraligi: `${event.start.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })} - ${event.end.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })}`,
                    derslikBilgisi: derslikBilgisi
                });
            });

            // Tabloyu oluştur
            const tableData = Array.from(sinavMap.values()).map(sinav => [
                { text: sinav.dersAdi },
                { text: sinav.ogretimUyesi },
                { text: sinav.sinavTarihi },
                { text: sinav.saatAraligi },
                { text: sinav.derslikBilgisi }
            ]);

            const headerRow = [
                { text: 'Ders Adı', style: 'tableHeader' },
                { text: 'Öğretim Üyesi', style: 'tableHeader' },
                { text: 'Sınav Tarihi', style: 'tableHeader' },
                { text: 'Saat Aralığı', style: 'tableHeader' },
                { text: 'Derslik ve Gözetmen', style: 'tableHeader' }
            ];

            const docDefinition = {
                pageOrientation: 'landscape',
                pageMargins: [20, 60, 20, 40],
                header: {
                    columns: [
                        {
                            text: 'SINAV TAKVİMİ',
                            alignment: 'left',
                            margin: [20, 20, 0, 0],
                            fontSize: 18,
                            bold: true
                        },
                        {
                            text: `Oluşturma Tarihi: ${new Date().toLocaleDateString('tr-TR')}`,
                            alignment: 'right',
                            margin: [0, 25, 20, 0],
                            fontSize: 10
                        }
                    ]
                },
                footer: function(currentPage, pageCount) {
                    return {
                        text: `Sayfa ${currentPage} / ${pageCount}`,
                        alignment: 'right',
                        margin: [0, 0, 20, 20]
                    };
                },
                content: [
                    {
                        table: {
                            headerRows: 1,
                            widths: ['*', '*', 'auto', 'auto', '*'],
                            body: [headerRow, ...tableData]
                        }
                    }
                ],
                styles: {
                    tableHeader: {
                        bold: true,
                        fontSize: 11,
                        color: 'white',
                        fillColor: '#337ab7',
                        alignment: 'center'
                    }
                },
                defaultStyle: {
                    font: 'Roboto',
                    fontSize: 10
                }
            };

            // PDF oluştur ve indir
            const fileName = `Sinav_Takvimi_${new Date().toLocaleDateString('tr-TR').replace(/\./g, '-')}.pdf`;
            pdfMake.createPdf(docDefinition).download(fileName);

        } catch (error) {
            console.error('PDF oluşturma hatası:', error);
            Swal.fire({
                title: 'Hata!',
                text: 'PDF oluşturulurken bir hata oluştu.',
                icon: 'error'
            });
        }
    }

    /**
     * Takvim verilerini Excel'e aktarır
     */
    exportToExcel() {
        // Tüm etkinlikleri al
        const events = calendar.getEvents();
        
        // Excel için veri hazırla
        const excelData = events.map(event => {
            const props = event.extendedProps;
            const derslik = this.dersliklerData.find(d => d.id === props.derslikId);
            const gozetmen = this.akademikPersonellerData.find(ap => ap.id === props.gozetmenId);
            
            return {
                'Ders': props.dersAd,
                'Öğretim Görevlisi': `${props.unvan} ${props.akademikPersonelAd}`,
                'Bölüm': props.bolumAd,
                'Tarih': event.start.toLocaleDateString('tr-TR'),
                'Saat': event.start.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' }),
                'Derslik': derslik ? derslik.ad : 'Atanmamış',
                'Derslik Kapasitesi': derslik ? derslik.kapasite : '-',
                'Gözetmen': gozetmen ? `${gozetmen.unvan} ${gozetmen.ad}` : '-'
            };
        });

        // Excel dosyası oluştur
        const ws = XLSX.utils.json_to_sheet(excelData);
        const wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, "Sınav Programı");

        // Sütun genişliklerini ayarla
        const maxWidth = Object.keys(excelData[0]).reduce((acc, key) => Math.max(acc, key.length), 0);
        ws['!cols'] = Object.keys(excelData[0]).map(() => ({ wch: maxWidth }));

        // Haftanın başlangıç ve bitiş tarihlerini al
        const currentDate = calendar.getDate();
        const monday = new Date(currentDate);
        monday.setDate(monday.getDate() - monday.getDay() + 1); // Pazartesi
        const friday = new Date(currentDate);
        friday.setDate(friday.getDate() - friday.getDay() + 5); // Cuma

        // İki basamaklı tarih formatı için yardımcı fonksiyon
        const pad = (num) => num.toString().padStart(2, '0');

        // Dosya adını oluştur
        const fileName = `Sinav_Programi_${pad(monday.getDate())}-${pad(monday.getMonth() + 1)}_${pad(friday.getDate())}-${pad(friday.getMonth() + 1)}.xlsx`;

        XLSX.writeFile(wb, fileName);
    }
}

// Sayfa yüklendiğinde takvim yöneticisini başlat
$(document).ready(() => {
    new SinavTakvimiManager(INITIAL_DATA);
}); 