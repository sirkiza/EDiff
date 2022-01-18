# EDiff
## Условие
Создайте простейший механизм расчета различий (диффа) между двумя бинарными файлами. Также создайте механизм, позволяющий “накатить” изменения на старую версию (проапдейтив ее до новой версии), используя файл старой версии и дифф между старой и новой версиями. 

## Интерфейс

```
SYNOPSIS
    .\EDiff.exe diff <path> <path>
    .\EDiff.exe patch <path_to_file> <path_to_ediff_output>

EDiff supports 2 types of commands:
    diff        Compares 2 files and produce EDiff output;
    patch       Update old file using EDiff output;
```

## Запуск

Для запуска нужно установить [.net6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

```
cd EDiff/
dotnet run diff text1.txt text2.txt > diff.txt
dotnet run patch text1.txt diff.txt
```
