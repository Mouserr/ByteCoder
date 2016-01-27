# ByteCoder

Консольное приложение, работающее в нескольких режимах, в зависимости от передаваемых аргументов:

1) –f filename –m find –s hello

Выводит все смещения в байтах (через пробел) для файла «filename», где расположена строка «hello»

2) –f filename –m checksum

Разбивает содержимое файла на 32-битные "машинные слова" и выводит их сумму. В случае нехватки байт для последнего слова, считает их равными нулю.

3) –h

Выводит справку о командах и параметрах