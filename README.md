# DumbledoreMapper

A very simple static mapper

## Install

`Install-Package DumbledoreMapperStandard -Version 1.3.1`

## How to use

`var viewModel = Mapper.Map<ViewModel>(model);`
  
`var model = Mapper.Map<Model>(viewModel);`

`var resultList = Mapper.Map<Model>(viewModelCollection);`

`Mapper.CopyProperties(source, target);`

