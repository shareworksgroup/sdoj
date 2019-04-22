﻿namespace solution {
    class Dom {
        private languageSelect = $("select[name='Language']");
        private code = ace.edit($(".code")[0], {
            mode: "ace/mode/javascript"
        });
        private hiddenCode = $("input[name='Source']");
        private form = <HTMLFormElement>document.getElementById("codeForm");

        constructor() {
            this.form.addEventListener("submit", () => {
                this.hiddenCode.val(this.code.getValue());
            });
        }

        getSelectedLanguage() {
            return <Languages>parseInt(this.languageSelect.val());
        }

        setSelectedLanguage(language: Languages) {
            this.languageSelect.val(language.toString());
        }

        setSourceCode(mode: string, text: string) {
            this.code.session.setMode(mode);
            this.code.setValue(text);
        }

        onLanguageChange(onchange: () => void) {
            this.languageSelect.change(() => onchange());
        }
    }

    let dom = new Dom();
    let store = new PerferedLanguageStore();

    dom.setSelectedLanguage(localStorage.perferedLanguage || Languages.csharp);
    dom.onLanguageChange(() => {
        localStorage.perferedLanguage = store.get();
        dom.setSourceCode(languageToAceMode(dom.getSelectedLanguage()), getLanguageTemplate(dom.getSelectedLanguage()));
    });
    dom.setSourceCode(languageToAceMode(dom.getSelectedLanguage()), getLanguageTemplate(dom.getSelectedLanguage()));
}