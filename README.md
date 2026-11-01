# RageLib: GTA 5 OPT 

Zero-allocation optimized fork of **RageLib** for Grand Theft Auto V (PC).  
Designed for ultra-fast reading, writing, and cryptographic processing of `.rpf` archives and RSC7 game resources.

[English](#english) | [Русский](#русский)

---

<a name="english"></a>
## English

### 1. Benchmark: `update.rpf` (1,920.88 MB / 1.92 GB)

Tested on the original GTA V `update.rpf` archive (5 iterations, warmup included):

| Metric | Original RageLib | Optimized RageLib | Difference / Improvement |
| :--- | :--- | :--- | :--- |
| **Header Read Time** | 3.22 ms | **1.12 ms** | **-65.2% time (2.87x faster)** |
| **Data Read Time** | 848.30 ms | **337.15 ms** | **-511.15 ms (2.52x faster)** |
| **Total Archive Read Time** | 851.52 ms | **338.28 ms** | **-513.24 ms (-60.3% total time)** |
| **Read Throughput** | 2,264.12 MB/s | **5,696.65 MB/s** | **+3,432.53 MB/s (~5.7 GB/s)** |
| **Heap Memory Allocated** | 1,974,380 KB (~1.97 GB) | **135,440 KB (~132 MB)** | **-1,838,939 KB (-93.1% allocations / -1.8 GB)** |
| **GC Gen0 Collections** | 58 pauses | **43 pauses** | **-15 GC pauses avoided** |

#### Pure Cryptography Benchmark: NG Decryption (64 KB block, 1,000 iterations)
* **Original:** `2,538 ms` (~25 MB/s)
* **Optimized:** **`156 ms` (~420 MB/s)**
* **Improvement: 16.27x faster (+1,527%)**

---

### 2. Implemented Optimizations

#### Zero-Allocation Cryptography (`RageLib.GTA5/Cryptography/GTA5Encryption.cs`)
* **Eliminated Block Allocations:** The original implementation allocated 48 heap objects (`new byte[16]`, `new uint[4]`, `new uint[17][]`) for every 16-byte cryptographic block. Replaced with in-place stack/register variables and subkey reuse.
* **Precomputed Round Keys:** Key expansion is performed once per buffer instead of being recomputed per block.
* **Multi-threaded Decryption:** Large cryptographic blocks (>512 blocks) are decrypted in parallel using `Parallel.For`.
* **In-Place API:** Added `DecryptInPlace` allowing direct buffer decryption without cloning memory.

#### Zero-Allocation Primitive I/O (`RageLib/Data/DataReader.cs`, `DataWriter.cs`)
* **Bitwise Conversion:** Replaced all `BitConverter.GetBytes()` and temporary `byte[]` allocations with an internal 8-byte union and bitwise shifts (`<<`, `>>`).
* **High-Throughput Resource Readers:** Added `ReadBytesInternal` and `WriteBytesInternal` in `ResourceDataReader` and `ResourceDataWriter` for zero-copy stream operations.

#### Chunked Streaming & Large Object Heap Protection (`RageLib.GTA5/ArchiveWrappers/RageArchiveWrapper7.cs`)
* **Stream Copying:** Replaced `new byte[fileSize]` per-file allocations in `Export` and `Import` with 128 KB chunked streaming (`binaryStream.CopyTo(stream, 131072)`).
* **LOH Isolation:** Prevents temporary multi-megabyte buffers from polluting the Large Object Heap (LOH), eliminating OutOfMemory risks and cutting GC heap allocations on a 2 GB archive from 1.97 GB down to 132 MB (-93.1%).

#### Optimized Hashing (`RageLib.GTA5/Cryptography/Jenkins.cs`, `GTA5Hash.cs`)
* **Byte-Level Hashing:** Replaced string conversions with direct byte-array hashing.
* **Safe LUT Masking:** Added `& 0xFF` masking preventing index out-of-bounds exceptions on non-ASCII paths.

#### Deflate Reliability (`RageLib/Compression/DeflateCompression.cs`, `ResourceFile_GTA5_pc.cs`)
* **Guaranteed Decompression:** Implemented `ReadFully` stream loops to handle non-compliant or fragmented zlib streams, preventing silent resource corruption.

#### Fast Space Allocation (`RageLib/Archives/ArchiveHelpers.cs`, `RageArchive7.cs`)
* **Buffer Enlargement:** Archive move buffer enlarged from 4 KB to 2 MB (500x fewer I/O syscalls during defragmentation and archive rebuilding).
* **Sorted Space Lookup:** Replaced quadratic block lookup with optimized sorting in `FindSpace`.

---

<a name="русский"></a>
## Русский

### 1. Результаты бенчмарка: `update.rpf` (1 920.88 МБ / 1.92 ГБ)

Тестирование на оригинальном архиве GTA V `update.rpf` (5 итераций с прогревом JIT):

| Метрика | Оригинальный RageLib | Оптимизированный RageLib | Разница / Улучшение |
| :--- | :--- | :--- | :--- |
| **Чтение заголовка** | 3.22 мс | **1.12 мс** | **-65.2% времени (в 2.87 раза быстрее)** |
| **Чтение файлов** | 848.30 мс | **337.15 мс** | **-511.15 мс (в 2.52 раза быстрее)** |
| **Общее время чтения** | 851.52 мс | **338.28 мс** | **-513.24 мс (-60.3% времени)** |
| **Скорость чтения** | 2 264.12 МБ/с | **5 696.65 МБ/с** | **+3 432.53 МБ/сек (~5.7 ГБ/сек)** |
| **Выделено памяти на куче** | 1 974 380 КБ (~1.97 ГБ) | **135 440 КБ (~132 МБ)** | **-1 838 939 КБ (-93.1% памяти / -1.8 ГБ)** |
| **Сборки мусора GC Gen0** | 58 сборок | **43 сборки** | **-15 пауз сборщика мусора** |

#### Чистый тест криптографии: NG-дешифрование (64 КБ блок, 1 000 итераций)
* **Оригинал:** `2 538 мс` (~25 МБ/сек)
* **Оптимизированный:** **`156 мс` (~420 МБ/сек)**
* **Результат: в 16.27 раз быстрее (+1 527%)**

---

### 2. Что было сделано

#### Криптография без аллокаций (`RageLib.GTA5/Cryptography/GTA5Encryption.cs`)
* **Устранение аллокаций в циклах:** Оригинальный алгоритм выделял 48 объектов в управляемой куче (`new byte[16]`, `new uint[4]`, `new uint[17][]`) на каждые 16 байт данных. Все раунды переписаны на регистровые переменные без единого выделения памяти.
* **Предвычисленные подключи:** Развертывание ключей выполняется один раз на весь буфер вместо каждого блока.
* **Параллельная обработка:** Для блоков больше 512 задействован `Parallel.For`.
* **In-Place методы:** Добавлен `DecryptInPlace`, выполняющий расшифровку прямо в исходном буфере.

#### Чтение примитивов без аллокаций (`RageLib/Data/DataReader.cs`, `DataWriter.cs`)
* **Побитовые сдвиги:** Устранены постоянные вызовы `BitConverter.GetBytes()` и создание временных массивов `new byte[]` при чтении/записи каждого `uint`, `int`, `float`, `ulong`. Использован внутренний 8-байтный буфер и побитовая арифметика.
* **Прямой доступ к потокам:** В `ResourceDataReader` и `ResourceDataWriter` добавлены методы `ReadBytesInternal` и `WriteBytesInternal` для работы без промежуточного копирования.

#### Потоковое копирование и защита Large Object Heap (`RageLib.GTA5/ArchiveWrappers/RageArchiveWrapper7.cs`)
* **Чанковый экспорт/импорт:** Заменен наивный код `var buf = new byte[stream.Length]` в методах `Export` и `Import` на чанковое копирование `binaryStream.CopyTo(stream, 131072)` блоками по 128 КБ.
* **Разгрузка кучи LOH:** На 2 ГБ архиве это предотвращает выброс 1.97 ГБ временных массивов в Large Object Heap, устраняя риск `OutOfMemoryException` и срезая аллокации памяти на **93.1%**.

#### Оптимизация хэширования (`RageLib.GTA5/Cryptography/Jenkins.cs`, `GTA5Hash.cs`)
* **Побайтное хэширование:** Реализована прямая перегрузка алгоритма Jenkins по массиву байт без конвертации в строки.
* **Безопасная маска LUT:** Добавлена маска `& 0xFF`, предотвращающая вылет за границы таблицы `PC_LUT` на путях с не-ASCII символами.

#### Надежность Deflate-декомпрессии (`RageLib/Compression/DeflateCompression.cs`, `ResourceFile_GTA5_pc.cs`)
* **Корректное чтение потоков:** Реализован строгий цикл `ReadFully`, гарантирующий чтение фрагментированных потоков `DeflateStream` без повреждения системных данных RSC7 ресурсов.

#### Ускорение реорганизации архива (`RageLib/Archives/ArchiveHelpers.cs`, `RageArchive7.cs`)
* **Увеличение буфера перемещения:** Буфер `MoveBytes` увеличен с 4 КБ до 2 МБ (в 500 раз меньше системных вызовов I/O при дефрагментации и перепаковке).
* **Быстрый поиск свободного места:** Оптимизирован компаратор сортировки в `FindSpace`.

---

### 3. Структура проекта

```text
ragelib-gta5-main/
├── RageLib/             # Базовые классы, потоки данных, DataReader/Writer, сжатие Deflate
├── RageLib.GTA5/        # Архитектура GTA 5: RPFv7, RSC7, шифрование AES/NG, Jenkins, ресурсы
└── README.md            # Документация и результаты бенчмарков
```
