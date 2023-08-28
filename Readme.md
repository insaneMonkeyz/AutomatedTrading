## Краткий путеводитель для ознакомления с моей работой с кодом

Проект предоставляет доступ к московской бирже через терминал Quik используя его Lua C Api интерфейс

Фасад функциональности проекта представлен интерфейсом [IQuik](https://github.com/insaneMonkeyz/AutomatedTrading/blob/master/Connectors/Quik/QuikLuaApiWrapper/IQuik.cs)

Реализуют его классы в файлах 
[Quik.cs](https://github.com/insaneMonkeyz/AutomatedTrading/blob/master/Connectors/Quik/QuikLuaApiWrapper/Quik.cs) и 
[IQuikImplementation.cs](https://github.com/insaneMonkeyz/AutomatedTrading/blob/master/Connectors/Quik/QuikLuaApiWrapper/IQuikImplementation.cs)

Обмен данными ведется через череду классов прослоек, абстрагирующих работу c низкоуровневыми конструкциями Lua:

1) Самый высокий уровень абстракции - вышеупомянутый интерфейс [IQuik](https://github.com/insaneMonkeyz/AutomatedTrading/blob/master/Connectors/Quik/QuikLuaApiWrapper/IQuik.cs) выступающий проводником к основной логике приложения - выставление заявок, получение информации по инструментам, ценам итд

2) Следующий слой - [провайдеры данных](https://github.com/insaneMonkeyz/AutomatedTrading/tree/master/Connectors/Quik/QuikLuaApiWrapper/EntityProviders). Здесь объекты бизнес сущностей приложения ([сделок, заявок, торговых инструментов, котировок итд](https://github.com/insaneMonkeyz/AutomatedTrading/tree/master/Connectors/Quik/QuikLuaApiWrapper/Entities)) 
собираются и разбираются для загрузки в терминал. 

3) Еще более глубокий слой абстракции - [Обертка для бизнес API терминала](https://github.com/insaneMonkeyz/AutomatedTrading/tree/master/Connectors/Quik/QuikLuaApiWrapper/EntityProviders/QuikApiWrappers). Здесь объекты нашего приложения переводятся в DTO объектов терминала.

4) Последний слой - [Обёртка для Lua C API](https://github.com/insaneMonkeyz/AutomatedTrading/tree/master/Connectors/Quik/QuikLuaApiWrapper/Lua). Представляет собой логику оперирующую стеком Lua в памяти терминала. Через него осуществляется обмен данными с терминалом.