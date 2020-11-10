// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// THIS FILE REQUIRES JQUERY AND LODASH TO WORK AT RUNTIME

let volunteerConvergence = (function () {

    let highlightFields = function (response) {

        $('.form-control').removeClass('is-invalid');

        $.each(response,
            function (propName, val) {
                propName = propName[0].toUpperCase() + propName.slice(1); //avoid auto-lowercased first letter
                let nameSelector = '[name = "' + propName.replace(/(:|\.|\[|\])/g, "_") + '"]',
                    idSelector = '#' + propName.replace(/(:|\.|\[|\])/g, "_");
                const $el = $(nameSelector).length > 1 ? $(nameSelector) : $(idSelector);

                if (val.Errors.length > 0) {
                    $el.closest('.form-control').addClass('is-invalid');
                }
            });
    };

    const showSummary = function (response) {
        $('#validationSummary').empty().removeClass('hidden');

        let verboseErrors = _.flatten(_.map(response, 'Errors')),
            errors = [];

        const nonNullErrors = _.reject(verboseErrors,
            function (error) {
                return error.ErrorMessage.indexOf('must not be empty.') > -1 ||
                    error.ErrorMessage.indexOf('is required.') > -1;
            });

        _.each(nonNullErrors,
            function (error) {
                errors.push(error.ErrorMessage);
            });

        if (nonNullErrors.length !== verboseErrors.length) {
            errors.push('The highlighted fields are required to submit this form.');
        }

        let $ul = $('#validationSummary').append('<ul></ul>');

        _.each(errors,
            function (error) {
                const $li = $('<li></li>').text(error);
                $li.appendTo($ul);
            });
    };

    const showServerError = function() {
        $('#validationSummary').empty().removeClass('hidden');
        let $ul = $('#validationSummary').append('<ul></ul>');
        const $li = $('<li></li>').text("There was an unexpected server error processing your request.");
        $li.appendTo($ul);
    };

    const highlightErrors = function (xhr) {
        try {
            if (xhr.status === 500) {
                showServerError();
                return;
            }

            const data = JSON.parse(xhr.responseText);
            highlightFields(data);
            showSummary(data);
            window.scrollTo(0, 0);
        } catch (e) {
            console.log(e);
        }
    };

    const redirect = function (data) {
        setTimeout(function () {
            if (data.redirect) {
                window.location = data.redirect;
            } else {
                window.scrollTo(0, 0);
                window.location.reload();
            }

        }, 10);
    };

    const doAjaxPost = function ($form, url) {
        var submitBtn = $form.find('[type="submit"]');

        submitBtn.prop('disabled', true);
        $(window).unbind();

        var formData = $form.serialize();

        $form.find('div').removeClass('has-error');

        $('body').addClass('loading');
        $.ajax({
            url: url,
            type: 'post',
            data: formData,
            contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
            dataType: 'json',
            cache: false
        })
            .done(function (data) {
                redirect(data);
            })
            .fail(function (xhr) {
                highlightErrors(xhr);
                submitBtn.prop('disabled', false);
                $('body').removeClass('loading');
            });
    };

    const ajaxPost = function (form, url) {
        doAjaxPost(form, url);
    };

    return {
        redirect: redirect,
        highlightErrors: highlightErrors,
        ajaxPost: ajaxPost
    }
}());


(function () {
    // Hijack form posts where we want to handle server side validation errors on the client side.
    $('form.with-ajax[method=post]').on('submit', function () {
        var $form = $(this);
        var url = $form.attr('action');
        volunteerConvergence.ajaxPost($form, url);
        return false;
    });
})();